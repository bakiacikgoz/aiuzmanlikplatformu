using AiEducation.Api.Data;
using AiEducation.Api.Features.Leagues;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class AdminController(AppDbContext db, LeagueService leagueService) : ApiControllerBase
{
    [HttpGet("content")]
    public async Task<IActionResult> Content(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            LearningPaths = await db.LearningPaths.CountAsync(cancellationToken),
            Units = await db.Units.CountAsync(cancellationToken),
            Lessons = await db.Lessons.CountAsync(cancellationToken),
            Resources = await db.Resources.CountAsync(cancellationToken),
            Badges = await db.Badges.CountAsync(cancellationToken),
            Quests = await db.Quests.CountAsync(cancellationToken),
            NotificationTemplates = await db.NotificationTemplates.CountAsync(cancellationToken)
        });
    }

    [HttpPost("learning-paths")]
    public async Task<IActionResult> CreatePath(AdminPathRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<LearningPath>(request.Slug, db.LearningPaths, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var path = new LearningPath
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Title = request.Title,
            Level = request.Level,
            Description = request.Description ?? "",
            SortOrder = request.SortOrder
        };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/learning-paths/{path.Slug}", new { path.Id, path.Slug });
    }

    [HttpGet("learning-paths")]
    public async Task<IActionResult> LearningPaths(CancellationToken cancellationToken)
    {
        var paths = await db.LearningPaths
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.Slug, x.Title, x.Level, x.Description, x.SortOrder })
            .ToListAsync(cancellationToken);
        return Ok(paths);
    }

    [HttpPatch("learning-paths/{id:guid}")]
    public async Task<IActionResult> UpdatePath(Guid id, AdminPathPatchRequest request, CancellationToken cancellationToken)
    {
        var path = await db.LearningPaths.FindAsync([id], cancellationToken);
        if (path is null)
        {
            return NotFound();
        }

        path.Title = request.Title ?? path.Title;
        path.Level = request.Level ?? path.Level;
        path.Description = request.Description ?? path.Description;
        path.SortOrder = request.SortOrder ?? path.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { path.Id, path.Slug });
    }

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit(AdminUnitRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<Unit>(request.Slug, db.Units, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var path = await db.LearningPaths.SingleOrDefaultAsync(x => x.Slug == request.LearningPathSlug, cancellationToken);
        if (path is null)
        {
            return BadRequest(new { message = "Yol haritası bulunamadı." });
        }

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            LearningPathId = path.Id,
            Slug = request.Slug,
            Title = request.Title,
            SortOrder = request.SortOrder
        };
        db.Units.Add(unit);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/learning-paths/{path.Slug}", new { unit.Id, unit.Slug });
    }

    [HttpGet("units")]
    public async Task<IActionResult> Units(CancellationToken cancellationToken)
    {
        var units = await db.Units
            .Include(x => x.LearningPath)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.Slug, x.Title, x.SortOrder, LearningPathSlug = x.LearningPath!.Slug })
            .ToListAsync(cancellationToken);
        return Ok(units);
    }

    [HttpPatch("units/{id:guid}")]
    public async Task<IActionResult> UpdateUnit(Guid id, AdminUnitPatchRequest request, CancellationToken cancellationToken)
    {
        var unit = await db.Units.FindAsync([id], cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        unit.Title = request.Title ?? unit.Title;
        unit.SortOrder = request.SortOrder ?? unit.SortOrder;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { unit.Id, unit.Slug });
    }

    [HttpPost("lessons")]
    public async Task<IActionResult> CreateLesson(AdminLessonRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<Lesson>(request.Slug, db.Lessons, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var unit = await db.Units.SingleOrDefaultAsync(x => x.Slug == request.UnitSlug, cancellationToken);
        if (unit is null)
        {
            return BadRequest(new { message = "Modül bulunamadı." });
        }

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            Slug = request.Slug,
            Title = request.Title,
            LearningObjective = request.LearningObjective,
            MiniExplanation = request.MiniExplanation ?? "",
            TinyExample = request.TinyExample ?? "",
            NextStep = request.NextStep ?? "",
            DurationMinutes = request.DurationMinutes,
            XpReward = request.XpReward,
            SortOrder = request.SortOrder,
            Status = ParseStatus(request.Status, ContentStatus.Draft),
            IsArchived = ParseStatus(request.Status, ContentStatus.Draft) == ContentStatus.Archived,
            Exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Prompt = request.ExercisePrompt,
                Type = ExerciseType.ShortAnswer,
                RequiresAiFeedback = true
            }
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/lessons/{lesson.Id}", ToAdminLessonPayload(lesson));
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> Lessons(CancellationToken cancellationToken)
    {
        var lessons = await db.Lessons
            .Include(x => x.Unit)
            .Include(x => x.Exercise)
            .Include(x => x.LessonResources)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
        return Ok(lessons.Select(ToAdminLessonPayload));
    }

    [HttpGet("lessons/{id:guid}")]
    public async Task<IActionResult> Lesson(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(x => x.Unit)
            .Include(x => x.Exercise)
            .Include(x => x.LessonResources)
            .ThenInclude(x => x.Resource)
            .Include(x => x.QuizQuestions)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return lesson is null ? NotFound() : Ok(ToAdminLessonPayload(lesson));
    }

    [HttpPatch("lessons/{id:guid}")]
    public async Task<IActionResult> UpdateLesson(Guid id, AdminLessonPatchRequest request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(x => x.Exercise)
            .Include(x => x.LessonResources)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        lesson.Title = request.Title ?? lesson.Title;
        lesson.LearningObjective = request.LearningObjective ?? lesson.LearningObjective;
        lesson.MiniExplanation = request.MiniExplanation ?? lesson.MiniExplanation;
        lesson.TinyExample = request.TinyExample ?? lesson.TinyExample;
        lesson.NextStep = request.NextStep ?? lesson.NextStep;
        lesson.DurationMinutes = request.DurationMinutes ?? lesson.DurationMinutes;
        lesson.XpReward = request.XpReward ?? lesson.XpReward;
        lesson.SortOrder = request.SortOrder ?? lesson.SortOrder;
        if (!string.IsNullOrWhiteSpace(request.ExercisePrompt))
        {
            if (lesson.Exercise is null)
            {
                lesson.Exercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    LessonId = lesson.Id,
                    Type = ExerciseType.ShortAnswer,
                    RequiresAiFeedback = true
                };
            }

            lesson.Exercise.Prompt = request.ExercisePrompt;
        }

        if (request.ResourceIds is not null)
        {
            db.LessonResources.RemoveRange(lesson.LessonResources);
            foreach (var resourceId in request.ResourceIds.Distinct())
            {
                if (await db.Resources.AnyAsync(x => x.Id == resourceId, cancellationToken))
                {
                    db.LessonResources.Add(new LessonResource { LessonId = lesson.Id, ResourceId = resourceId });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToAdminLessonPayload(lesson));
    }

    [HttpPatch("lessons/{id:guid}/status")]
    public async Task<IActionResult> UpdateLessonStatus(Guid id, AdminLessonStatusRequest request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(x => x.Exercise)
            .Include(x => x.LessonResources)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        var status = ParseStatus(request.Status, lesson.Status);
        if (status == ContentStatus.Published)
        {
            var issues = ValidatePublishReadiness(lesson).ToArray();
            if (issues.Length > 0)
            {
                return BadRequest(new { message = "Ders yayın kalite kapısından geçemedi.", issues, qualityScore = CalculateQualityScore(lesson) });
            }
        }

        lesson.Status = status;
        lesson.IsArchived = status == ContentStatus.Archived;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToAdminLessonPayload(lesson));
    }

    [HttpGet("lessons/{id:guid}/preview")]
    public async Task<IActionResult> LessonPreview(Guid id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(x => x.Exercise)
            .Include(x => x.LessonResources)
            .ThenInclude(x => x.Resource)
            .Include(x => x.QuizQuestions)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            lesson.Id,
            lesson.Slug,
            lesson.Title,
            lesson.DurationMinutes,
            lesson.XpReward,
            lesson.LearningObjective,
            lesson.MiniExplanation,
            lesson.TinyExample,
            lesson.NextStep,
            lesson.Status,
            QualityScore = CalculateQualityScore(lesson)
        });
    }

    [HttpGet("lessons/{id:guid}/exercises")]
    public async Task<IActionResult> LessonExercises(Guid id, CancellationToken cancellationToken)
    {
        var exercises = await db.Exercises
            .Where(x => x.LessonId == id)
            .Select(x => new { x.Id, x.LessonId, Type = x.Type.ToString(), x.Prompt, x.CorrectAnswer, x.AutoGradable, x.RequiresAiFeedback })
            .ToListAsync(cancellationToken);
        return Ok(exercises);
    }

    [HttpPost("lessons/{id:guid}/exercises")]
    public async Task<IActionResult> CreateExercise(Guid id, AdminExerciseRequest request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.Include(x => x.Exercise).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { message = "Exercise prompt zorunludur." });
        }

        if (lesson.Exercise is not null)
        {
            return Conflict(new { message = "Bu dersin zaten bir alıştırması var." });
        }

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            LessonId = lesson.Id,
            Type = ParseExerciseType(request.Type),
            Prompt = request.Prompt,
            CorrectAnswer = request.CorrectAnswer,
            AutoGradable = request.AutoGradable,
            RequiresAiFeedback = request.RequiresAiFeedback
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/exercises/{exercise.Id}", new { exercise.Id });
    }

    [HttpPatch("exercises/{id:guid}")]
    public async Task<IActionResult> UpdateExercise(Guid id, AdminExerciseRequest request, CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises.FindAsync([id], cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        exercise.Type = ParseExerciseType(request.Type);
        exercise.Prompt = request.Prompt;
        exercise.CorrectAnswer = request.CorrectAnswer;
        exercise.AutoGradable = request.AutoGradable;
        exercise.RequiresAiFeedback = request.RequiresAiFeedback;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { exercise.Id });
    }

    [HttpDelete("exercises/{id:guid}")]
    public async Task<IActionResult> DeleteExercise(Guid id, CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises.FindAsync([id], cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        db.Exercises.Remove(exercise);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("lessons/{id:guid}/quiz-questions")]
    public async Task<IActionResult> QuizQuestions(Guid id, CancellationToken cancellationToken)
    {
        var questions = await db.QuizQuestions
            .Include(x => x.Options)
            .Where(x => x.LessonId == id && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                QuestionText = x.Prompt,
                x.QuestionType,
                x.SortOrder,
                x.Explanation,
                Options = x.Options.OrderBy(o => o.SortOrder).Select(o => new { o.Id, o.Text, o.SortOrder, o.IsCorrect })
            })
            .ToListAsync(cancellationToken);
        return Ok(questions);
    }

    [HttpPost("lessons/{id:guid}/quiz-questions")]
    public async Task<IActionResult> CreateQuizQuestion(Guid id, AdminQuizQuestionRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Lessons.AnyAsync(x => x.Id == id, cancellationToken))
        {
            return NotFound();
        }

        var validation = ValidateQuizQuestion(request);
        if (validation is not null)
        {
            return validation;
        }

        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            LessonId = id,
            Prompt = request.QuestionText,
            QuestionType = request.QuestionType ?? "multiple_choice",
            Explanation = request.Explanation ?? "",
            SortOrder = request.SortOrder,
            IsActive = true,
            Options = request.Options.Select(option => new QuizOption
            {
                Id = Guid.NewGuid(),
                Text = option.Text,
                IsCorrect = option.IsCorrect,
                SortOrder = option.SortOrder
            }).ToList()
        };
        db.QuizQuestions.Add(question);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/quiz-questions/{question.Id}", new { question.Id });
    }

    [HttpPatch("quiz-questions/{id:guid}")]
    public async Task<IActionResult> UpdateQuizQuestion(Guid id, AdminQuizQuestionRequest request, CancellationToken cancellationToken)
    {
        var question = await db.QuizQuestions.Include(x => x.Options).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        var validation = ValidateQuizQuestion(request);
        if (validation is not null)
        {
            return validation;
        }

        question.Prompt = request.QuestionText;
        question.QuestionType = request.QuestionType ?? "multiple_choice";
        question.Explanation = request.Explanation ?? "";
        question.SortOrder = request.SortOrder;
        db.QuizOptions.RemoveRange(question.Options);
        question.Options = request.Options.Select(option => new QuizOption
        {
            Id = Guid.NewGuid(),
            QuizQuestionId = question.Id,
            Text = option.Text,
            IsCorrect = option.IsCorrect,
            SortOrder = option.SortOrder
        }).ToList();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { question.Id });
    }

    [HttpDelete("quiz-questions/{id:guid}")]
    public async Task<IActionResult> DeleteQuizQuestion(Guid id, CancellationToken cancellationToken)
    {
        var question = await db.QuizQuestions.FindAsync([id], cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        question.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("resources")]
    public async Task<IActionResult> CreateResource(AdminResourceRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<Resource>(request.Slug, db.Resources, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            return BadRequest(new { message = "Kaynak URL'si http/https olmalıdır." });
        }

        var resource = new Resource { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Url = request.Url, Type = request.Type, Summary = request.Summary ?? "" };
        db.Resources.Add(resource);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/resources/{resource.Slug}", new { resource.Id, resource.Slug });
    }

    [HttpGet("resources")]
    public async Task<IActionResult> Resources(CancellationToken cancellationToken)
    {
        var resources = await db.Resources
            .OrderBy(x => x.Title)
            .Select(x => new { x.Id, x.Slug, x.Title, x.Url, x.Type, x.Summary })
            .ToListAsync(cancellationToken);
        return Ok(resources);
    }

    [HttpPatch("resources/{id:guid}")]
    public async Task<IActionResult> UpdateResource(Guid id, AdminResourcePatchRequest request, CancellationToken cancellationToken)
    {
        var resource = await db.Resources.FindAsync([id], cancellationToken);
        if (resource is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.Url) && (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
        {
            return BadRequest(new { message = "Kaynak URL'si http/https olmalıdır." });
        }

        resource.Title = request.Title ?? resource.Title;
        resource.Url = request.Url ?? resource.Url;
        resource.Type = request.Type ?? resource.Type;
        resource.Summary = request.Summary ?? resource.Summary;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { resource.Id, resource.Slug });
    }

    [HttpPost("leagues/rollover")]
    public async Task<IActionResult> RolloverLeagues(CancellationToken cancellationToken)
    {
        await leagueService.RolloverIfNeededAsync(cancellationToken);
        return Ok(new { rolledOver = true });
    }

    [HttpPost("leagues/{seasonId:guid}/close")]
    public async Task<IActionResult> CloseLeagueSeason(Guid seasonId, CancellationToken cancellationToken)
    {
        await leagueService.CloseSeasonAsync(seasonId, cancellationToken);
        return Ok(new { closed = true });
    }

    [HttpPost("badges")]
    public async Task<IActionResult> CreateBadge(AdminBadgeRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<Badge>(request.Slug, db.Badges, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var badge = new Badge { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Condition = request.Condition };
        db.Badges.Add(badge);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/badges/{badge.Id}", new { badge.Id, badge.Slug });
    }

    [HttpPost("quests")]
    public async Task<IActionResult> CreateQuest(AdminQuestRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<Quest>(request.Slug, db.Quests, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var quest = new Quest { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Cadence = request.Cadence, RewardXp = request.RewardXp, RewardGems = request.RewardGems };
        db.Quests.Add(quest);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/quests/{quest.Id}", new { quest.Id, quest.Slug });
    }

    [HttpPost("notification-templates")]
    public async Task<IActionResult> CreateNotificationTemplate(AdminNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateSlugAsync<NotificationTemplate>(request.Slug, db.NotificationTemplates, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var template = new NotificationTemplate { Id = Guid.NewGuid(), Slug = request.Slug, Channel = request.Channel, Title = request.Title, Body = request.Body };
        db.NotificationTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/notification-templates/{template.Id}", new { template.Id, template.Slug });
    }

    private async Task<IActionResult?> ValidateSlugAsync<TEntity>(string slug, DbSet<TEntity> set, CancellationToken cancellationToken)
        where TEntity : class
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            return BadRequest(new { message = "Slug kebab-case olmalıdır." });
        }

        var exists = await set.AnyAsync(x => EF.Property<string>(x, "Slug") == slug, cancellationToken);
        return exists ? Conflict(new { message = "Bu slug zaten kullanılıyor." }) : null;
    }

    private static ContentStatus ParseStatus(string? value, ContentStatus fallback)
    {
        return Enum.TryParse<ContentStatus>(value, ignoreCase: true, out var parsed) ? parsed : fallback;
    }

    private static ExerciseType ParseExerciseType(string? value)
    {
        return Enum.TryParse<ExerciseType>(value, ignoreCase: true, out var parsed) ? parsed : ExerciseType.ShortAnswer;
    }

    private static object ToAdminLessonPayload(Lesson lesson)
    {
        return new
        {
            lesson.Id,
            lesson.Slug,
            lesson.Title,
            lesson.UnitId,
            UnitSlug = lesson.Unit?.Slug,
            Status = lesson.Status.ToString(),
            lesson.IsArchived,
            lesson.LearningObjective,
            EstimatedMinutes = lesson.DurationMinutes,
            lesson.MiniExplanation,
            MiniExample = lesson.TinyExample,
            lesson.NextStep,
            ExercisePrompt = lesson.Exercise?.Prompt,
            ResourceIds = lesson.LessonResources.Select(x => x.ResourceId),
            lesson.XpReward,
            lesson.SortOrder,
            QualityScore = CalculateQualityScore(lesson)
        };
    }

    private static int CalculateQualityScore(Lesson lesson)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(lesson.LearningObjective)) score += 10;
        if (lesson.DurationMinutes is >= 1 and <= 10) score += 10;
        if (!string.IsNullOrWhiteSpace(lesson.MiniExplanation) && WordCount(lesson.MiniExplanation) <= 180) score += 15;
        if (!string.IsNullOrWhiteSpace(lesson.TinyExample)) score += 15;
        if (lesson.Exercise is not null) score += 20;
        if (lesson.Exercise is not null && (lesson.Exercise.RequiresAiFeedback || !string.IsNullOrWhiteSpace(lesson.Exercise.CorrectAnswer))) score += 10;
        if (lesson.LessonResources.Count > 0) score += 10;
        if (!string.IsNullOrWhiteSpace(lesson.NextStep)) score += 10;
        return score;
    }

    private static IEnumerable<string> ValidatePublishReadiness(Lesson lesson)
    {
        if (string.IsNullOrWhiteSpace(lesson.Title)) yield return "Başlık zorunludur.";
        if (string.IsNullOrWhiteSpace(lesson.Slug)) yield return "Slug zorunludur.";
        if (string.IsNullOrWhiteSpace(lesson.LearningObjective)) yield return "Öğrenme hedefi zorunludur.";
        if (lesson.DurationMinutes is < 1 or > 10) yield return "Süre 1-10 dakika arasında olmalıdır.";
        if (string.IsNullOrWhiteSpace(lesson.MiniExplanation)) yield return "Mini açıklama zorunludur.";
        if (WordCount(lesson.MiniExplanation) > 180) yield return "Mini açıklama 180 kelimeyi geçmemelidir.";
        if (string.IsNullOrWhiteSpace(lesson.TinyExample)) yield return "Mini örnek zorunludur.";
        if (lesson.Exercise is null) yield return "En az bir alıştırma zorunludur.";
        if (lesson.LessonResources.Count == 0) yield return "En az bir kaynak zorunludur.";
        if (lesson.XpReward <= 0) yield return "XP değeri pozitif olmalıdır.";
        if (lesson.SortOrder <= 0) yield return "Sıralama değeri pozitif olmalıdır.";
        if (lesson.UnitId is null) yield return "Unit zorunludur.";
        if (CalculateQualityScore(lesson) < 70) yield return "Kalite skoru en az 70 olmalıdır.";
    }

    private IActionResult? ValidateQuizQuestion(AdminQuizQuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.QuestionText))
        {
            return BadRequest(new { message = "Soru metni zorunludur." });
        }

        if (request.Options.Length < 2)
        {
            return BadRequest(new { message = "Çoktan seçmeli soruda en az 2 seçenek olmalıdır." });
        }

        if (request.Options.Any(x => string.IsNullOrWhiteSpace(x.Text)))
        {
            return BadRequest(new { message = "Seçenek metinleri boş olamaz." });
        }

        if (request.Options.Count(x => x.IsCorrect) != 1)
        {
            return BadRequest(new { message = "Her soruda tam olarak 1 doğru seçenek olmalıdır." });
        }

        return null;
    }

    private static int WordCount(string value)
    {
        return value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }
}

public sealed record AdminPathRequest(string Slug, string Title, string Level, string? Description, int SortOrder);
public sealed record AdminPathPatchRequest(string? Title, string? Level, string? Description, int? SortOrder);
public sealed record AdminUnitRequest(string LearningPathSlug, string Slug, string Title, int SortOrder);
public sealed record AdminUnitPatchRequest(string? Title, int? SortOrder);
public sealed record AdminLessonRequest(string UnitSlug, string Slug, string Title, string LearningObjective, string? MiniExplanation, string? TinyExample, string? NextStep, string ExercisePrompt, int DurationMinutes, int XpReward, int SortOrder, string? Status);
public sealed record AdminLessonPatchRequest(string? Title, string? LearningObjective, string? MiniExplanation, string? TinyExample, string? NextStep, string? ExercisePrompt, int? DurationMinutes, int? XpReward, int? SortOrder, Guid[]? ResourceIds);
public sealed record AdminLessonStatusRequest(string Status);
public sealed record AdminExerciseRequest(string? Type, string Prompt, string? CorrectAnswer, bool AutoGradable, bool RequiresAiFeedback);
public sealed record AdminQuizQuestionRequest(string QuestionText, string? QuestionType, int SortOrder, string? Explanation, AdminQuizOptionRequest[] Options);
public sealed record AdminQuizOptionRequest(string Text, bool IsCorrect, int SortOrder);
public sealed record AdminResourceRequest(string Slug, string Title, string Url, string Type, string? Summary);
public sealed record AdminResourcePatchRequest(string? Title, string? Url, string? Type, string? Summary);
public sealed record AdminBadgeRequest(string Slug, string Title, string Condition);
public sealed record AdminQuestRequest(string Slug, string Title, QuestCadence Cadence, int RewardXp, int RewardGems);
public sealed record AdminNotificationTemplateRequest(string Slug, string Channel, string Title, string Body);
