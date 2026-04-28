using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(AppDbContext db) : ApiControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var user = await db.Users.FindAsync([userId], cancellationToken);
        var pathSlug = user?.SelectedLearningPathSlug ?? "beginner";
        var today = await db.Lessons
            .Where(x => x.Unit!.LearningPath!.Slug == pathSlug && x.DailyEligible)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Slug, x.Title, x.DurationMinutes, x.XpReward, x.LearningObjective })
            .FirstOrDefaultAsync(cancellationToken);
        var totalXp = await db.XpTransactions.Where(x => x.UserId == userId).SumAsync(x => x.Amount, cancellationToken);
        var streak = await db.UserStreaks.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        var submissions = await db.PortfolioEvidence.CountAsync(x => x.UserId == userId, cancellationToken);

        return Ok(new
        {
            User = new { user?.DisplayName, user?.DailyXpGoal, user?.SelectedLearningPathSlug },
            TodayAiByte = today,
            TotalXp = totalXp,
            CurrentStreakDays = streak?.CurrentStreakDays ?? 0,
            PortfolioEvidenceCount = submissions
        });
    }
}
