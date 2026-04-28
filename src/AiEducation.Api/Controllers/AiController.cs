using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.AiMentor;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/ai")]
[EnableRateLimiting("ai")]
public sealed class AiController(AppDbContext db, IAiMentorClient mentorClient, AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(AiRequest request, CancellationToken cancellationToken)
    {
        return await StoreAndRespondAsync(request, cancellationToken);
    }

    [HttpPost("explain-lesson")]
    public async Task<IActionResult> ExplainLesson(AiRequest request, CancellationToken cancellationToken)
    {
        var lesson = request.LessonSlug is null ? null : await db.Lessons.SingleOrDefaultAsync(x => x.Slug == request.LessonSlug, cancellationToken);
        return await StoreAndRespondAsync(request with { Context = lesson?.LearningObjective }, cancellationToken);
    }

    [HttpPost("review-submission")]
    public async Task<IActionResult> ReviewSubmission(AiRequest request, CancellationToken cancellationToken)
    {
        return await StoreAndRespondAsync(request with { Context = "Rubrik: teknik doğruluk, uygulanabilirlik, değerlendirme, dokümantasyon, portföy sunumu." }, cancellationToken);
    }

    [HttpPost("generate-practice")]
    public async Task<IActionResult> GeneratePractice(AiRequest request, CancellationToken cancellationToken)
    {
        return await StoreAndRespondAsync(request with { Context = "Yeni bir kısa pratik üret." }, cancellationToken);
    }

    private async Task<IActionResult> StoreAndRespondAsync(AiRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 2000)
        {
            return BadRequest(new { message = "AI mentor mesajı 1-2000 karakter arasında olmalıdır." });
        }

        var userId = CurrentUserId();
        var monthlyLimit = await MonthlyAiLimitAsync(userId, cancellationToken);
        var monthStart = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var conversations = await db.AiConversations
            .Include(x => x.Messages)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        var used = conversations
            .SelectMany(x => x.Messages)
            .Count(x => x.Role == "user" && x.CreatedAtUtc >= monthStart);
        if (used >= monthlyLimit)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "AI mentor aylık mesaj limitine ulaşıldı." });
        }

        var response = await mentorClient.RespondAsync(new AiMentorRequest(request.Message, request.LessonSlug, request.Context), cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var conversation = new AiConversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LessonId = request.LessonSlug is null ? null : await db.Lessons.Where(x => x.Slug == request.LessonSlug).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken),
            CreatedAtUtc = now,
            Messages =
            [
                new AiMessage { Id = Guid.NewGuid(), Role = "user", Content = request.Message, CreatedAtUtc = now },
                new AiMessage { Id = Guid.NewGuid(), Role = "assistant", Content = response.Content, CreatedAtUtc = now }
            ]
        };
        db.AiConversations.Add(conversation);
        await db.SaveChangesAsync(cancellationToken);
        await analyticsService.TrackAsync(userId, AnalyticsEvents.AIMessageSent, new
        {
            conversation.Id,
            request.LessonSlug,
            used = used + 1,
            monthlyLimit
        }, now, cancellationToken);
        return Ok(new
        {
            conversation.Id,
            response.Content,
            response.SafetyNote,
            response.SuggestedNextStep,
            response.Confidence,
            response.RubricScores
        });
    }

    private async Task<int> MonthlyAiLimitAsync(Guid userId, CancellationToken cancellationToken)
    {
        var limit = await db.UserSubscriptions
            .Where(x => x.UserId == userId && x.Status == "active")
            .Join(db.SubscriptionPlans, subscription => subscription.SubscriptionPlanId, plan => plan.Id, (_, plan) => plan.MonthlyAiMessageLimit)
            .FirstOrDefaultAsync(cancellationToken);

        if (limit > 0)
        {
            return limit;
        }

        return await db.SubscriptionPlans
            .Where(x => x.Slug == "free")
            .Select(x => x.MonthlyAiMessageLimit)
            .FirstOrDefaultAsync(cancellationToken) is var freeLimit && freeLimit > 0 ? freeLimit : 10;
    }
}

public sealed record AiRequest(string Message, string? LessonSlug = null, string? Context = null);
