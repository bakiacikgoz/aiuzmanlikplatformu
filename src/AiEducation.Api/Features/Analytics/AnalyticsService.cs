using System.Text.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Models;

namespace AiEducation.Api.Features.Analytics;

public static class AnalyticsEvents
{
    public const string UserSignedUp = "UserSignedUp";
    public const string UserLoggedIn = "UserLoggedIn";
    public const string PathSelected = "PathSelected";
    public const string LessonStarted = "LessonStarted";
    public const string ExerciseSubmitted = "ExerciseSubmitted";
    public const string LessonCompleted = "LessonCompleted";
    public const string XPGranted = "XPGranted";
    public const string StreakExtended = "StreakExtended";
    public const string StreakBroken = "StreakBroken";
    public const string QuestCompleted = "QuestCompleted";
    public const string QuestClaimed = "QuestClaimed";
    public const string QuizSubmitted = "QuizSubmitted";
    public const string ProjectSubmitted = "ProjectSubmitted";
    public const string AIMessageSent = "AIMessageSent";
    public const string ExperimentAssigned = "ExperimentAssigned";
}

public sealed class AnalyticsService(AppDbContext db)
{
    public async Task TrackAsync(
        Guid? userId,
        string eventName,
        object? properties = null,
        DateTimeOffset? createdAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventName = eventName,
            PropertiesJson = JsonSerializer.Serialize(properties ?? new { }),
            CreatedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
