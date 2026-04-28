using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Gamification;

public sealed class QuestProgressService(AppDbContext db, AnalyticsService? analyticsService = null)
{
    public async Task<UserQuest?> MarkCompletedAsync(
        Guid userId,
        string questSlug,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var quest = await db.Quests.SingleOrDefaultAsync(x => x.Slug == questSlug, cancellationToken);
        if (quest is null)
        {
            return null;
        }

        var user = await db.Users.FindAsync([userId], cancellationToken);
        var localDate = StreakService.ToLocalDate(completedAtUtc, user?.TimeZoneId ?? "Europe/Istanbul");
        var userQuest = await db.UserQuests.SingleOrDefaultAsync(
            x => x.UserId == userId && x.QuestId == quest.Id && x.LocalDate == localDate,
            cancellationToken);

        if (userQuest is null)
        {
            userQuest = new UserQuest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                QuestId = quest.Id,
                LocalDate = localDate
            };
            db.UserQuests.Add(userQuest);
        }

        if (!userQuest.Completed)
        {
            userQuest.Completed = true;
            await db.SaveChangesAsync(cancellationToken);

            if (analyticsService is not null)
            {
                await analyticsService.TrackAsync(userId, AnalyticsEvents.QuestCompleted, new
                {
                    questSlug,
                    localDate
                }, completedAtUtc, cancellationToken);
            }
        }
        else
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return userQuest;
    }
}
