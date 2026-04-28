using AiEducation.Api.Data;
using AiEducation.Api.Features.AiMentor;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/ai")]
public sealed class AiController(AppDbContext db, IAiMentorClient mentorClient) : ApiControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(AiRequest request, CancellationToken cancellationToken)
    {
        return Ok(await StoreAndRespondAsync(request, cancellationToken));
    }

    [HttpPost("explain-lesson")]
    public async Task<IActionResult> ExplainLesson(AiRequest request, CancellationToken cancellationToken)
    {
        var lesson = request.LessonSlug is null ? null : await db.Lessons.SingleOrDefaultAsync(x => x.Slug == request.LessonSlug, cancellationToken);
        return Ok(await StoreAndRespondAsync(request with { Context = lesson?.LearningObjective }, cancellationToken));
    }

    [HttpPost("review-submission")]
    public async Task<IActionResult> ReviewSubmission(AiRequest request, CancellationToken cancellationToken)
    {
        return Ok(await StoreAndRespondAsync(request with { Context = "Rubrik: teknik doğruluk, uygulanabilirlik, değerlendirme, dokümantasyon, portföy sunumu." }, cancellationToken));
    }

    [HttpPost("generate-practice")]
    public async Task<IActionResult> GeneratePractice(AiRequest request, CancellationToken cancellationToken)
    {
        var response = await mentorClient.RespondAsync(new AiMentorRequest(request.Message, request.LessonSlug, "Yeni bir kısa pratik üret."), cancellationToken);
        return Ok(new { prompt = response.Content, response.SafetyNote });
    }

    private async Task<object> StoreAndRespondAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var response = await mentorClient.RespondAsync(new AiMentorRequest(request.Message, request.LessonSlug, request.Context), cancellationToken);
        var conversation = new AiConversation
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId(),
            LessonId = request.LessonSlug is null ? null : await db.Lessons.Where(x => x.Slug == request.LessonSlug).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Messages =
            [
                new AiMessage { Id = Guid.NewGuid(), Role = "user", Content = request.Message, CreatedAtUtc = DateTimeOffset.UtcNow },
                new AiMessage { Id = Guid.NewGuid(), Role = "assistant", Content = response.Content, CreatedAtUtc = DateTimeOffset.UtcNow }
            ]
        };
        db.AiConversations.Add(conversation);
        await db.SaveChangesAsync(cancellationToken);
        return new { conversation.Id, response.Content, response.SafetyNote };
    }
}

public sealed record AiRequest(string Message, string? LessonSlug = null, string? Context = null);
