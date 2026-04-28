using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/onboarding")]
public sealed class OnboardingController(AppDbContext db) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Complete(OnboardingRequest request, CancellationToken cancellationToken)
    {
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
        return Ok(new { user.DailyXpGoal, user.SelectedLearningPathSlug });
    }
}

public sealed record OnboardingRequest(int DailyXpGoal, string LearningPathSlug);
