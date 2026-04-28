using Microsoft.AspNetCore.Identity;

namespace AiEducation.Api.Models;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "AI Learner";
    public string TimeZoneId { get; set; } = "Europe/Istanbul";
    public int DailyXpGoal { get; set; } = 20;
    public string? SelectedLearningPathSlug { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? OnboardedAtUtc { get; set; }
    public int CachedTotalXp { get; set; }
}

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public ApplicationUser? User { get; set; }
}

public sealed class LearningPath
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Level { get; set; } = "beginner";
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
    public List<Unit> Units { get; set; } = [];
}

public sealed class Unit
{
    public Guid Id { get; set; }
    public Guid LearningPathId { get; set; }
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public int SortOrder { get; set; }
    public LearningPath? LearningPath { get; set; }
    public List<Lesson> Lessons { get; set; } = [];
}

public sealed class Lesson
{
    public Guid Id { get; set; }
    public Guid? UnitId { get; set; }
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string LessonType { get; set; } = "micro_lesson";
    public int DurationMinutes { get; set; } = 4;
    public int XpReward { get; set; } = 10;
    public bool DailyEligible { get; set; } = true;
    public string Difficulty { get; set; } = "easy";
    public string LearningObjective { get; set; } = "";
    public string MiniExplanation { get; set; } = "";
    public string TinyExample { get; set; } = "";
    public string CompletionCriteriaJson { get; set; } = "[]";
    public int PassingScorePercent { get; set; } = 70;
    public int QuestionCount { get; set; }
    public int SortOrder { get; set; }
    public Unit? Unit { get; set; }
    public Exercise? Exercise { get; set; }
    public List<LessonResource> LessonResources { get; set; } = [];
}

public sealed class Exercise
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public ExerciseType Type { get; set; } = ExerciseType.ShortAnswer;
    public string Prompt { get; set; } = default!;
    public bool AutoGradable { get; set; }
    public bool RequiresAiFeedback { get; set; }
    public string? CorrectAnswer { get; set; }
    public Lesson? Lesson { get; set; }
}

public enum ExerciseType
{
    MultipleChoice,
    ShortAnswer,
    CodeFill,
    Debugging,
    PromptRewrite
}

public sealed class Resource
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Url { get; set; } = "";
    public string Type { get; set; } = "resource";
    public string Summary { get; set; } = "";
}

public sealed class LessonResource
{
    public Guid LessonId { get; set; }
    public Guid ResourceId { get; set; }
    public Lesson? Lesson { get; set; }
    public Resource? Resource { get; set; }
}

public sealed class UserPathEnrollment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LearningPathId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ApplicationUser? User { get; set; }
    public LearningPath? LearningPath { get; set; }
}

public sealed class UserLessonProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public bool ExerciseSubmitted { get; set; }
    public int? ScorePercent { get; set; }
    public ApplicationUser? User { get; set; }
    public Lesson? Lesson { get; set; }
}

public sealed class QuizAttempt
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public int ScorePercent { get; set; }
    public string WrongAnswersJson { get; set; } = "[]";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Lesson? Lesson { get; set; }
}

public sealed class Project
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Level { get; set; } = "beginner";
    public string Title { get; set; } = default!;
    public string Duration { get; set; } = "";
    public int XpReward { get; set; } = 120;
    public string DeliverablesJson { get; set; } = "[]";
    public List<ProjectRubricCriterion> RubricCriteria { get; set; } = [];
}

public sealed class ProjectRubricCriterion
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Criterion { get; set; } = default!;
    public int Points { get; set; }
    public Project? Project { get; set; }
}

public sealed class ProjectSubmission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public string RepositoryUrl { get; set; } = "";
    public string DemoUrl { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Project? Project { get; set; }
    public List<PortfolioEvidence> PortfolioEvidence { get; set; } = [];
}

public sealed class PortfolioEvidence
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectSubmissionId { get; set; }
    public string Title { get; set; } = default!;
    public string EvidenceUrl { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ProjectSubmission? ProjectSubmission { get; set; }
}

public sealed class XpTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EventType { get; set; } = default!;
    public int Amount { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool PassedQualityGate { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class XpRule
{
    public Guid Id { get; set; }
    public string Event { get; set; } = default!;
    public int Xp { get; set; }
    public int DailyCap { get; set; }
    public string QualityGate { get; set; } = "";
}

public sealed class UserStreak
{
    public Guid UserId { get; set; }
    public int CurrentStreakDays { get; set; }
    public int LongestStreakDays { get; set; }
    public DateOnly? LastCompletedLocalDate { get; set; }
    public int FreezeCount { get; set; }
    public int MonthlyRecoveryCount { get; set; }
    public ApplicationUser? User { get; set; }
}

public sealed class Badge
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Condition { get; set; } = "";
}

public sealed class UserBadge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BadgeId { get; set; }
    public DateTimeOffset AwardedAtUtc { get; set; }
    public Badge? Badge { get; set; }
}

public sealed class Quest
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public QuestCadence Cadence { get; set; } = QuestCadence.Daily;
    public int RewardXp { get; set; }
    public int RewardGems { get; set; }
}

public enum QuestCadence
{
    Daily,
    Weekly
}

public sealed class UserQuest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestId { get; set; }
    public DateOnly LocalDate { get; set; }
    public bool Completed { get; set; }
    public bool Claimed { get; set; }
    public Quest? Quest { get; set; }
}

public sealed class LeagueSeason
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public LeagueTier Tier { get; set; } = LeagueTier.Bronze;
    public List<LeagueParticipant> Participants { get; set; } = [];
}

public sealed class LeagueParticipant
{
    public Guid Id { get; set; }
    public Guid LeagueSeasonId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = "Learner";
    public LeagueTier LeagueTier { get; set; } = LeagueTier.Bronze;
    public int WeeklyXp { get; set; }
    public int Rank { get; set; }
    public DateTimeOffset JoinedAtUtc { get; set; }
    public LeagueSeason? LeagueSeason { get; set; }
}

public enum LeagueTier
{
    Bronze,
    Silver,
    Gold,
    Sapphire,
    Ruby,
    Emerald,
    Diamond
}

public enum LeagueMovement
{
    Promote,
    Stay,
    Demote
}

public sealed record LeagueResult(Guid UserId, int Rank, LeagueTier CurrentTier, LeagueMovement Movement);

public sealed class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string Channel { get; set; } = "web";
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
}

public sealed class NotificationLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? NotificationTemplateId { get; set; }
    public string Status { get; set; } = "sent";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public NotificationTemplate? NotificationTemplate { get; set; }
}

public sealed class Experiment
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Hypothesis { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string PrimaryMetric { get; set; } = "";
    public string GuardrailMetric { get; set; } = "";
    public List<ExperimentVariant> Variants { get; set; } = [];
}

public sealed class ExperimentVariant
{
    public Guid Id { get; set; }
    public Guid ExperimentId { get; set; }
    public string Key { get; set; } = default!;
    public int Weight { get; set; } = 1;
    public Experiment? Experiment { get; set; }
}

public sealed class ExperimentAssignment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ExperimentId { get; set; }
    public Guid ExperimentVariantId { get; set; }
    public string ExperimentKey { get; set; } = default!;
    public string VariantKey { get; set; } = default!;
    public DateTimeOffset AssignedAtUtc { get; set; }
}

public sealed class AnalyticsEvent
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string EventName { get; set; } = default!;
    public string PropertiesJson { get; set; } = "{}";
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class AiConversation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? LessonId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<AiMessage> Messages { get; set; } = [];
}

public sealed class AiMessage
{
    public Guid Id { get; set; }
    public Guid AiConversationId { get; set; }
    public string Role { get; set; } = "assistant";
    public string Content { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public AiConversation? Conversation { get; set; }
}

public sealed class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "free";
    public string Name { get; set; } = "Free";
    public int MonthlyAiMessageLimit { get; set; } = 10;
}

public sealed class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAtUtc { get; set; }
}
