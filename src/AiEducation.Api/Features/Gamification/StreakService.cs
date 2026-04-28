using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Gamification;

public sealed class StreakService(AppDbContext db, AnalyticsService? analyticsService = null)
{
    public async Task<UserStreak> ApplyLearningActivityAsync(
        Guid userId,
        DateTimeOffset activityAtUtc,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var streak = await db.UserStreaks.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (streak is null)
        {
            streak = new UserStreak { UserId = userId };
            db.UserStreaks.Add(streak);
        }

        var localDate = ToLocalDate(activityAtUtc, user.TimeZoneId);

        if (streak.LastCompletedLocalDate == localDate)
        {
            return streak;
        }

        var previousDate = streak.LastCompletedLocalDate;
        var wasBroken = previousDate is not null && previousDate != localDate.AddDays(-1);
        if (streak.LastCompletedLocalDate == localDate.AddDays(-1))
        {
            streak.CurrentStreakDays += 1;
        }
        else
        {
            streak.CurrentStreakDays = 1;
        }

        streak.LongestStreakDays = Math.Max(streak.LongestStreakDays, streak.CurrentStreakDays);
        streak.LastCompletedLocalDate = localDate;
        await db.SaveChangesAsync(cancellationToken);

        if (analyticsService is not null)
        {
            if (wasBroken)
            {
                await analyticsService.TrackAsync(userId, AnalyticsEvents.StreakBroken, new
                {
                    previousDate,
                    localDate
                }, activityAtUtc, cancellationToken);
            }

            await analyticsService.TrackAsync(userId, AnalyticsEvents.StreakExtended, new
            {
                localDate,
                streak.CurrentStreakDays,
                streak.LongestStreakDays
            }, activityAtUtc, cancellationToken);
        }

        return streak;
    }

    public async Task<UserStreak?> ApplyDailyGoalIfMetAsync(
        Guid userId,
        DateTimeOffset activityAtUtc,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            return null;
        }

        var localDate = ToLocalDate(activityAtUtc, user.TimeZoneId);
        var (startUtc, endUtc) = UtcRangeForLocalDate(localDate, user.TimeZoneId);
        var candidates = await db.XpTransactions
            .Where(x => x.UserId == userId
                && x.PassedQualityGate)
            .ToListAsync(cancellationToken);
        var dailyXp = candidates
            .Where(x => x.CreatedAtUtc >= startUtc && x.CreatedAtUtc < endUtc)
            .Sum(x => x.Amount);

        return dailyXp >= user.DailyXpGoal
            ? await ApplyLearningActivityAsync(userId, activityAtUtc, cancellationToken)
            : null;
    }

    public static DateOnly ToLocalDate(DateTimeOffset utc, string timeZoneId)
    {
        var timezone = FindTimeZone(timeZoneId);
        var local = TimeZoneInfo.ConvertTime(utc, timezone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    private static TimeZoneInfo FindTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException) when (timeZoneId == "Europe/Istanbul")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch (InvalidTimeZoneException) when (timeZoneId == "Europe/Istanbul")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }

    private static (DateTimeOffset StartUtc, DateTimeOffset EndUtc) UtcRangeForLocalDate(DateOnly localDate, string timeZoneId)
    {
        var timezone = FindTimeZone(timeZoneId);
        var startLocal = localDate.ToDateTime(TimeOnly.MinValue);
        var endLocal = localDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return (
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(startLocal, timezone), TimeSpan.Zero),
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endLocal, timezone), TimeSpan.Zero));
    }
}
