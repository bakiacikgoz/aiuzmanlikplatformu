using AiEducation.Api.Data;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Features.Leagues;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1")]
public sealed class GamificationController(AppDbContext db, XpService xpService, LeagueService leagueService) : ApiControllerBase
{
    [HttpGet("gamification/me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var user = await db.Users.FindAsync([userId], cancellationToken);
        var totalXp = await db.XpTransactions.Where(x => x.UserId == userId).SumAsync(x => x.Amount, cancellationToken);
        var streak = await db.UserStreaks.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        var localDate = StreakService.ToLocalDate(DateTimeOffset.UtcNow, user?.TimeZoneId ?? "Europe/Istanbul");
        var quests = await DailyQuestPayloads(userId, localDate, cancellationToken);

        return Ok(new
        {
            TotalXp = totalXp,
            CurrentStreakDays = streak?.CurrentStreakDays ?? 0,
            LongestStreakDays = streak?.LongestStreakDays ?? 0,
            DailyGoal = user?.DailyXpGoal ?? 20,
            DailyQuests = quests
        });
    }

    [HttpGet("quests/daily")]
    public async Task<IActionResult> DailyQuests(CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([CurrentUserId()], cancellationToken);
        var localDate = StreakService.ToLocalDate(DateTimeOffset.UtcNow, user?.TimeZoneId ?? "Europe/Istanbul");
        return Ok(await DailyQuestPayloads(CurrentUserId(), localDate, cancellationToken));
    }

    [HttpPost("quests/{id:guid}/claim")]
    public async Task<IActionResult> ClaimQuest(Guid id, CancellationToken cancellationToken)
    {
        var userQuest = await db.UserQuests.Include(x => x.Quest).SingleOrDefaultAsync(
            x => x.UserId == CurrentUserId() && x.QuestId == id,
            cancellationToken);
        if (userQuest is null || !userQuest.Completed)
        {
            return BadRequest(new { message = "Görev henüz tamamlanmadı." });
        }

        if (!userQuest.Claimed)
        {
            userQuest.Claimed = true;
            await db.SaveChangesAsync(cancellationToken);
            await xpService.GrantXpAsync(CurrentUserId(), XpEvents.QuestClaimed, userQuest.Quest?.RewardXp ?? 0, "Quest", id, true, DateTimeOffset.UtcNow, cancellationToken);
        }

        return Ok(new { claimed = true });
    }

    [HttpGet("badges/me")]
    public async Task<IActionResult> Badges(CancellationToken cancellationToken)
    {
        var badges = await db.UserBadges
            .Include(x => x.Badge)
            .Where(x => x.UserId == CurrentUserId())
            .Select(x => new { x.Badge!.Slug, x.Badge.Title, x.AwardedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(badges);
    }

    [HttpGet("leagues/current")]
    public async Task<IActionResult> CurrentLeague(CancellationToken cancellationToken)
    {
        var season = await leagueService.GetOrCreateCurrentSeasonAsync(CurrentUserId(), cancellationToken);
        var ranked = season.Participants
            .OrderByDescending(x => x.WeeklyXp)
            .Select((x, index) => new
            {
                Rank = index + 1,
                x.UserId,
                x.DisplayName,
                x.WeeklyXp,
                x.LeagueTier,
                IsCurrentUser = x.UserId == CurrentUserId()
            })
            .ToList();

        return Ok(new
        {
            season.Name,
            season.StartsAtUtc,
            season.EndsAtUtc,
            season.Tier,
            Participants = ranked
        });
    }

    private async Task<IReadOnlyList<object>> DailyQuestPayloads(Guid userId, DateOnly localDate, CancellationToken cancellationToken)
    {
        var quests = await db.Quests.Where(x => x.Cadence == QuestCadence.Daily).ToListAsync(cancellationToken);
        var questIds = quests.Select(x => x.Id).ToArray();
        var progress = await db.UserQuests
            .Where(x => x.UserId == userId && x.LocalDate == localDate && questIds.Contains(x.QuestId))
            .ToListAsync(cancellationToken);

        return quests.Select(quest =>
        {
            var userQuest = progress.FirstOrDefault(x => x.QuestId == quest.Id);
            return (object)new
            {
                quest.Id,
                quest.Slug,
                quest.Title,
                quest.RewardXp,
                quest.RewardGems,
                Completed = userQuest?.Completed ?? false,
                Claimed = userQuest?.Claimed ?? false
            };
        }).ToList();
    }
}
