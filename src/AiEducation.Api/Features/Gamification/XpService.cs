using AiEducation.Api.Data;
using AiEducation.Api.Models;

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

public sealed class XpService(AppDbContext db)
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

        var transaction = new XpTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            Amount = amount,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            PassedQualityGate = passedQualityGate,
            CreatedAtUtc = createdAtUtc
        };

        db.XpTransactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return transaction;
    }
}
