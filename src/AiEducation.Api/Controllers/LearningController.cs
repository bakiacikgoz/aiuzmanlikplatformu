using System.Text.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.Learning;
using AiEducation.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Route("api/v1")]
public sealed class LearningController(AppDbContext db, LessonCompletionService completionService, AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpGet("learning-paths")]
    public async Task<ActionResult<IReadOnlyList<object>>> Paths(CancellationToken cancellationToken)
    {
        var paths = await db.LearningPaths
            .Include(x => x.Units)
            .ThenInclude(x => x.Lessons)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Slug,
                x.Title,
                x.Level,
                x.Description,
                UnitCount = x.Units.Count,
                LessonCount = x.Units.SelectMany(u => u.Lessons).Count(l => l.Status == Models.ContentStatus.Published && !l.IsArchived)
            })
            .ToListAsync(cancellationToken);
        return Ok(paths);
    }

    [HttpGet("learning-paths/{slug}")]
    public async Task<ActionResult<object>> PathDetail(string slug, CancellationToken cancellationToken)
    {
        var path = await db.LearningPaths
            .Include(x => x.Units.OrderBy(u => u.SortOrder))
            .ThenInclude(x => x.Lessons.OrderBy(l => l.SortOrder))
            .SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        if (path is null)
        {
            return NotFound();
        }

        return new
        {
            path.Slug,
            path.Title,
            path.Level,
            path.Description,
            Units = path.Units.Select(unit => new
            {
                unit.Slug,
                unit.Title,
                Lessons = unit.Lessons
                    .Where(lesson => lesson.Status == Models.ContentStatus.Published && !lesson.IsArchived)
                    .Select(lesson => new
                {
                    lesson.Slug,
                    lesson.Title,
                    lesson.DurationMinutes,
                    lesson.XpReward,
                    lesson.DailyEligible
                })
            })
        };
    }

    [HttpGet("lessons/{slug}")]
    public async Task<ActionResult<object>> Lesson(string slug, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(x => x.Exercise)
            .Include(x => x.QuizQuestions)
            .ThenInclude(x => x.Options)
            .Include(x => x.LessonResources)
            .ThenInclude(x => x.Resource)
            .SingleOrDefaultAsync(x => x.Slug == slug && x.Status == Models.ContentStatus.Published && !x.IsArchived, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        return new
        {
            lesson.Slug,
            lesson.Title,
            lesson.DurationMinutes,
            lesson.XpReward,
            lesson.Difficulty,
            lesson.LearningObjective,
            Status = lesson.Status.ToString(),
            Exercise = lesson.Exercise is null ? null : new
            {
                lesson.Exercise.Id,
                Type = lesson.Exercise.Type.ToString(),
                lesson.Exercise.Prompt,
                lesson.Exercise.AutoGradable,
                lesson.Exercise.RequiresAiFeedback
            },
            Steps = BuildLessonSteps(lesson),
            Resources = lesson.LessonResources.Select(x => new
            {
                x.Resource!.Slug,
                x.Resource.Title,
                x.Resource.Url,
                x.Resource.Type
            }),
            QuizQuestions = lesson.QuizQuestions.Where(x => x.IsActive).OrderBy(x => x.SortOrder).Select(question => new
            {
                question.Id,
                question.Prompt,
                Options = question.Options.OrderBy(option => option.SortOrder).Select(option => new
                {
                    option.Id,
                    option.Text
                })
            })
        };
    }

    [HttpPost("lessons/{slug}/start")]
    [Authorize]
    public async Task<IActionResult> Start(string slug, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.SingleOrDefaultAsync(x => x.Slug == slug && x.Status == Models.ContentStatus.Published && !x.IsArchived, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        var userId = CurrentUserId();
        var progress = await db.UserLessonProgresses.SingleOrDefaultAsync(
            x => x.UserId == userId && x.LessonId == lesson.Id,
            cancellationToken);
        if (progress is null)
        {
            db.UserLessonProgresses.Add(new Models.UserLessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lesson.Id,
                StartedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        await analyticsService.TrackAsync(userId, AnalyticsEvents.LessonStarted, new
        {
            lesson.Slug,
            lesson.Id
        }, DateTimeOffset.UtcNow, cancellationToken);

        return Ok(new { started = true });
    }

    [HttpPost("lessons/{slug}/complete")]
    [Authorize]
    public async Task<IActionResult> Complete(string slug, CompleteLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await completionService.CompleteAsync(
            CurrentUserId(),
            slug,
            request.ExerciseSubmitted,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return result.Succeeded
            ? Ok(new { completed = true, result.XpGranted })
            : BadRequest(new { message = result.Error });
    }

    private static IReadOnlyList<LessonStep> BuildLessonSteps(Models.Lesson lesson)
    {
        var criteria = JsonSerializer.Deserialize<string[]>(lesson.CompletionCriteriaJson) ?? [];
        return
        [
            new("goal", "Bugünkü küçük hedef", lesson.LearningObjective),
            new("explanation", "Mini açıklama", string.IsNullOrWhiteSpace(lesson.MiniExplanation) ? $"{lesson.Title} konusunu tek kavram ve tek örnek üzerinden hızlıca öğren." : lesson.MiniExplanation),
            new("example", "Mini örnek", string.IsNullOrWhiteSpace(lesson.TinyExample) ? $"Örnek: {lesson.Title} gerçek bir AI ürününde küçük bir karar veya tahmin olarak karşına çıkar." : lesson.TinyExample),
            new("exercise", "Şimdi sen dene", lesson.Exercise?.Prompt ?? "Konuyu kendi cümlenle açıkla."),
            new("feedback", "Anında geri bildirim", lesson.Exercise?.RequiresAiFeedback == true ? "Cevabın AI mentor rubriğine göre değerlendirilecek." : "Cevabın otomatik kontrol edilir."),
            new("reward", "Ödül", $"{lesson.XpReward} XP kazanırsın. Kriterler: {string.Join(", ", criteria)}"),
            new("next", "Sonraki küçük adım", string.IsNullOrWhiteSpace(lesson.NextStep) ? "Bir sonraki AI Byte veya pratik görevine geç." : lesson.NextStep)
        ];
    }
}

public sealed record LessonStep(string Key, string Title, string Body);
public sealed record CompleteLessonRequest(bool ExerciseSubmitted, string? Answer);
