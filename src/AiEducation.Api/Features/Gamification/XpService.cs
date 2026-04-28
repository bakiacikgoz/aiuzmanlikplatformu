using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Gamification;

public static class XpEvents
{
    public const string MicroLessonCompleted = "micro_lesson_completed";
    public const string PracticeCompleted = "practice_completed";
    public const string QuizPassed = "quiz_passed";
    public const string ProjectStepSubmitted = "project_step_submitted";
    public const string ProjectSubmitted = "project_submitted";
    public const string QuestClaimed = "quest_claimed";
}

public sealed class XpService(AppDbContext db, AnalyticsService? analyticsService = null)
{
    public async Task<XpTransaction> GrantXpAsync(
        Guid userId,
        string eventType,
        int amount,
        string? referenceType,
        Guid? referenceId,
        bool passedQualityGate,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "XP amount must be positive.");
        }

        if (referenceId is not null)
        {
            var existing = await db.XpTransactions.FirstOrDefaultAsync(
                x => x.UserId == userId
                    && x.EventType == eventType
                    && x.ReferenceType == referenceType
                    && x.ReferenceId == referenceId,
                cancellationToken);
            if (existing is not null)
            {
                return existing;
            }
        }

        var rule = await db.XpRules.SingleOrDefaultAsync(x => x.Event == eventType, cancellationToken);
        var effectiveAmount = rule?.Xp > 0 ? rule.Xp : amount;
        var effectivePassedQualityGate = passedQualityGate;

        if (!passedQualityGate)
        {
            effectiveAmount = 0;
        }
        else if (rule?.DailyCap > 0)
        {
            var user = await db.Users.FindAsync([userId], cancellationToken);
            var localDate = StreakService.ToLocalDate(createdAtUtc, user?.TimeZoneId ?? "Europe/Istanbul");
            var (startUtc, endUtc) = UtcRangeForLocalDate(localDate, user?.TimeZoneId ?? "Europe/Istanbul");
            var todayCandidates = await db.XpTransactions
                .Where(x => x.UserId == userId
                    && x.EventType == eventType
                    && x.PassedQualityGate)
                .ToListAsync(cancellationToken);
            var alreadyGrantedToday = todayCandidates
                .Where(x => x.CreatedAtUtc >= startUtc && x.CreatedAtUtc < endUtc)
                .Sum(x => x.Amount);
            var remaining = Math.Max(0, rule.DailyCap - alreadyGrantedToday);
            effectiveAmount = Math.Min(effectiveAmount, remaining);
            effectivePassedQualityGate = effectiveAmount > 0;
        }

        var transaction = new XpTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            Amount = effectiveAmount,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            PassedQualityGate = effectivePassedQualityGate,
            CreatedAtUtc = createdAtUtc
        };

        db.XpTransactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        if (transaction.Amount > 0 && analyticsService is not null)
        {
            await analyticsService.TrackAsync(userId, AnalyticsEvents.XPGranted, new
            {
                eventType,
                amount = transaction.Amount,
                referenceType,
                referenceId
            }, createdAtUtc, cancellationToken);
        }

        return transaction;
    }

    public async Task<int> GetQualityXpTotalAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.XpTransactions
            .Where(x => x.UserId == userId && x.PassedQualityGate)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    private static (DateTimeOffset StartUtc, DateTimeOffset EndUtc) UtcRangeForLocalDate(DateOnly localDate, string timeZoneId)
    {
        var timeZone = FindTimeZone(timeZoneId);
        var startLocal = localDate.ToDateTime(TimeOnly.MinValue);
        var endLocal = localDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, timeZone);
        return (new DateTimeOffset(startUtc, TimeSpan.Zero), new DateTimeOffset(endUtc, TimeSpan.Zero));
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
