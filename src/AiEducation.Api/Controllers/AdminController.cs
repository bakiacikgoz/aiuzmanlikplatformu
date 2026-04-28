using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class AdminController(AppDbContext db) : ApiControllerBase
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
            DurationMinutes = request.DurationMinutes,
            XpReward = request.XpReward,
            SortOrder = request.SortOrder,
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
        return Created($"/api/v1/lessons/{lesson.Slug}", new { lesson.Id, lesson.Slug });
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
}

public sealed record AdminPathRequest(string Slug, string Title, string Level, string? Description, int SortOrder);
public sealed record AdminUnitRequest(string LearningPathSlug, string Slug, string Title, int SortOrder);
public sealed record AdminLessonRequest(string UnitSlug, string Slug, string Title, string LearningObjective, string? MiniExplanation, string? TinyExample, string ExercisePrompt, int DurationMinutes, int XpReward, int SortOrder);
public sealed record AdminResourceRequest(string Slug, string Title, string Url, string Type, string? Summary);
public sealed record AdminBadgeRequest(string Slug, string Title, string Condition);
public sealed record AdminQuestRequest(string Slug, string Title, QuestCadence Cadence, int RewardXp, int RewardGems);
public sealed record AdminNotificationTemplateRequest(string Slug, string Channel, string Title, string Body);
