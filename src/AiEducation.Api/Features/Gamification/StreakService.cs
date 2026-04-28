using AiEducation.Api.Data;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Gamification;

public sealed class StreakService(AppDbContext db)
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
        return streak;
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
}
