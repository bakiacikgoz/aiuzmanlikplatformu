using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/onboarding")]
public sealed class OnboardingController(AppDbContext db, AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Complete(OnboardingRequest request, CancellationToken cancellationToken)
    {
        if (request.DailyXpGoal is not (10 or 20 or 30 or 40 or 50))
        {
            return BadRequest(new { message = "Günlük XP hedefi 10, 20, 30, 40 veya 50 olmalıdır." });
        }

        var userId = CurrentUserId();
        var user = await db.Users.FindAsync([userId], cancellationToken);
        var path = await db.LearningPaths.SingleOrDefaultAsync(x => x.Slug == request.LearningPathSlug, cancellationToken);
        if (user is null || path is null)
        {
            return BadRequest(new { message = "Kullanıcı veya yol haritası bulunamadı." });
        }

        user.DailyXpGoal = request.DailyXpGoal;
        user.SelectedLearningPathSlug = path.Slug;
        user.OnboardedAtUtc ??= DateTimeOffset.UtcNow;

        var exists = await db.UserPathEnrollments.AnyAsync(x => x.UserId == userId && x.LearningPathId == path.Id, cancellationToken);
        if (!exists)
        {
            db.UserPathEnrollments.Add(new UserPathEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LearningPathId = path.Id,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        if (!await db.UserStreaks.AnyAsync(x => x.UserId == userId, cancellationToken))
        {
            db.UserStreaks.Add(new UserStreak { UserId = userId });
        }

        await db.SaveChangesAsync(cancellationToken);
        await analyticsService.TrackAsync(userId, AnalyticsEvents.PathSelected, new
        {
            pathSlug = path.Slug,
            request.DailyXpGoal
        }, DateTimeOffset.UtcNow, cancellationToken);
        return Ok(new { user.DailyXpGoal, user.SelectedLearningPathSlug });
    }
}

public sealed record OnboardingRequest(int DailyXpGoal, string LearningPathSlug);
