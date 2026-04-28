using System.Text.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1")]
public sealed class PracticeController(AppDbContext db, XpService xpService) : ApiControllerBase
{
    [HttpPost("exercises/{id:guid}/submit")]
    public async Task<IActionResult> SubmitExercise(Guid id, SubmitExerciseRequest request, CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises.Include(x => x.Lesson).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        await xpService.GrantXpAsync(CurrentUserId(), XpEvents.PracticeCompleted, 15, "Exercise", id, !string.IsNullOrWhiteSpace(request.Answer), DateTimeOffset.UtcNow, cancellationToken);
        return Ok(new { accepted = true, requiresAiFeedback = exercise.RequiresAiFeedback, feedback = exercise.RequiresAiFeedback ? "AI mentor cevabını kısa bir rubrikle değerlendirecek." : "Cevabın kaydedildi." });
    }

    [HttpPost("quizzes/{lessonSlug}/attempts")]
    public async Task<IActionResult> SubmitQuiz(string lessonSlug, QuizAttemptRequest request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.SingleOrDefaultAsync(x => x.Slug == lessonSlug, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        var attempt = new Models.QuizAttempt
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId(),
            LessonId = lesson.Id,
            ScorePercent = request.ScorePercent,
            WrongAnswersJson = JsonSerializer.Serialize(request.WrongAnswers),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        var xp = 0;
        if (request.ScorePercent >= lesson.PassingScorePercent)
        {
            xp = 25;
            await xpService.GrantXpAsync(CurrentUserId(), XpEvents.QuizPassed, xp, "QuizAttempt", attempt.Id, true, DateTimeOffset.UtcNow, cancellationToken);
        }

        return Ok(new { attempt.Id, passed = request.ScorePercent >= lesson.PassingScorePercent, xpGranted = xp });
    }

    [HttpGet("review/mistakes")]
    public async Task<ActionResult<IReadOnlyList<object>>> Mistakes(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var mistakes = await db.QuizAttempts
            .Include(x => x.Lesson)
            .Where(x => x.UserId == userId && x.WrongAnswersJson != "[]")
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .Select(x => new { x.Lesson!.Slug, x.Lesson.Title, x.WrongAnswersJson, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(mistakes);
    }
}

public sealed record SubmitExerciseRequest(string Answer);
public sealed record QuizAttemptRequest(int ScorePercent, string[] WrongAnswers);
