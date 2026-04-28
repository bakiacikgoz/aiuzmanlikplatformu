using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
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
        var resource = new Resource { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Url = request.Url, Type = request.Type, Summary = request.Summary ?? "" };
        db.Resources.Add(resource);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/resources/{resource.Slug}", new { resource.Id, resource.Slug });
    }

    [HttpPost("badges")]
    public async Task<IActionResult> CreateBadge(AdminBadgeRequest request, CancellationToken cancellationToken)
    {
        var badge = new Badge { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Condition = request.Condition };
        db.Badges.Add(badge);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/badges/{badge.Id}", new { badge.Id, badge.Slug });
    }

    [HttpPost("quests")]
    public async Task<IActionResult> CreateQuest(AdminQuestRequest request, CancellationToken cancellationToken)
    {
        var quest = new Quest { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Cadence = request.Cadence, RewardXp = request.RewardXp, RewardGems = request.RewardGems };
        db.Quests.Add(quest);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/quests/{quest.Id}", new { quest.Id, quest.Slug });
    }

    [HttpPost("notification-templates")]
    public async Task<IActionResult> CreateNotificationTemplate(AdminNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = new NotificationTemplate { Id = Guid.NewGuid(), Slug = request.Slug, Channel = request.Channel, Title = request.Title, Body = request.Body };
        db.NotificationTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/notification-templates/{template.Id}", new { template.Id, template.Slug });
    }
}

public sealed record AdminPathRequest(string Slug, string Title, string Level, string? Description, int SortOrder);
public sealed record AdminUnitRequest(string LearningPathSlug, string Slug, string Title, int SortOrder);
public sealed record AdminLessonRequest(string UnitSlug, string Slug, string Title, string LearningObjective, string? MiniExplanation, string? TinyExample, string ExercisePrompt, int DurationMinutes, int XpReward, int SortOrder);
public sealed record AdminResourceRequest(string Slug, string Title, string Url, string Type, string? Summary);
public sealed record AdminBadgeRequest(string Slug, string Title, string Condition);
public sealed record AdminQuestRequest(string Slug, string Title, QuestCadence Cadence, int RewardXp, int RewardGems);
public sealed record AdminNotificationTemplateRequest(string Slug, string Channel, string Title, string Body);
