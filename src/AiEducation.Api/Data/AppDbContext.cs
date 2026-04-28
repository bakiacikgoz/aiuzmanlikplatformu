using AiEducation.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<UserExerciseSubmission> UserExerciseSubmissions => Set<UserExerciseSubmission>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<LessonResource> LessonResources => Set<LessonResource>();
    public DbSet<UserPathEnrollment> UserPathEnrollments => Set<UserPathEnrollment>();
    public DbSet<UserLessonProgress> UserLessonProgresses => Set<UserLessonProgress>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectSubmission> ProjectSubmissions => Set<ProjectSubmission>();
    public DbSet<ProjectRubricCriterion> ProjectRubricCriteria => Set<ProjectRubricCriterion>();
    public DbSet<PortfolioEvidence> PortfolioEvidence => Set<PortfolioEvidence>();
    public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();
    public DbSet<XpRule> XpRules => Set<XpRule>();
    public DbSet<UserStreak> UserStreaks => Set<UserStreak>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<UserQuest> UserQuests => Set<UserQuest>();
    public DbSet<LeagueSeason> LeagueSeasons => Set<LeagueSeason>();
    public DbSet<LeagueParticipant> LeagueParticipants => Set<LeagueParticipant>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<Experiment> Experiments => Set<Experiment>();
    public DbSet<ExperimentVariant> ExperimentVariants => Set<ExperimentVariant>();
    public DbSet<ExperimentAssignment> ExperimentAssignments => Set<ExperimentAssignment>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(x => x.DisplayName)
            .HasMaxLength(160);

        builder.Entity<LearningPath>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Unit>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Lesson>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Lesson>().Property(x => x.IsArchived).HasDefaultValue(false);
        builder.Entity<Resource>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Project>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Badge>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<XpRule>().HasIndex(x => x.Event).IsUnique();
        builder.Entity<Quest>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Experiment>().HasIndex(x => x.Key).IsUnique();
        builder.Entity<NotificationTemplate>().HasIndex(x => x.Slug).IsUnique();

        builder.Entity<LessonResource>().HasKey(x => new { x.LessonId, x.ResourceId });
        builder.Entity<UserStreak>().HasKey(x => x.UserId);
        builder.Entity<UserStreak>()
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserStreak>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Exercise>()
            .HasOne(x => x.Lesson)
            .WithOne(x => x.Exercise)
            .HasForeignKey<Exercise>(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserExerciseSubmission>()
            .HasIndex(x => new { x.UserId, x.ExerciseId, x.CreatedAtUtc });

        builder.Entity<UserExerciseSubmission>()
            .HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserExerciseSubmission>()
            .HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizQuestion>()
            .HasOne(x => x.Lesson)
            .WithMany(x => x.QuizQuestions)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<QuizQuestion>().Property(x => x.QuestionType).HasDefaultValue("multiple_choice");
        builder.Entity<QuizQuestion>().Property(x => x.IsActive).HasDefaultValue(true);

        builder.Entity<QuizOption>()
            .HasOne(x => x.QuizQuestion)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<XpTransaction>()
            .HasIndex(x => new { x.UserId, x.EventType, x.ReferenceType, x.ReferenceId });

        builder.Entity<ExperimentAssignment>()
            .HasIndex(x => new { x.UserId, x.ExperimentKey })
            .IsUnique();

        builder.Entity<RefreshToken>()
            .HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.Entity<NotificationPreference>().HasKey(x => x.UserId);
        builder.Entity<NotificationPreference>()
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<NotificationPreference>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLessonProgress>()
            .HasIndex(x => new { x.UserId, x.LessonId })
            .IsUnique();

        builder.Entity<UserPathEnrollment>()
            .HasIndex(x => new { x.UserId, x.LearningPathId })
            .IsUnique();
    }
}
