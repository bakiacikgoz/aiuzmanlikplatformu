using AiEducation.Api.Data;
using AiEducation.Api.Features.Experimentation;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Features.Learning;
using AiEducation.Api.Features.Leagues;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Tests;

public sealed class CoreRulesTests
{
    [Fact]
    public async Task GrantXp_records_transaction_instead_of_mutating_user_total()
    {
        await using var db = TestDb.Create();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "learner@example.com",
            Email = "learner@example.com",
            TimeZoneId = "Europe/Istanbul",
            DailyXpGoal = 20
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new XpService(db);

        var transaction = await service.GrantXpAsync(
            user.Id,
            XpEvents.MicroLessonCompleted,
            10,
            "Lesson",
            Guid.NewGuid(),
            passedQualityGate: true,
            DateTimeOffset.Parse("2026-04-28T08:00:00Z"));

        Assert.Equal(10, transaction.Amount);
        Assert.True(transaction.PassedQualityGate);
        Assert.Equal(10, await db.XpTransactions.Where(x => x.UserId == user.Id).SumAsync(x => x.Amount));
        Assert.Equal(0, user.CachedTotalXp);
    }

    [Fact]
    public async Task Streak_uses_user_timezone_for_local_learning_day()
    {
        await using var db = TestDb.Create();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "learner@example.com",
            Email = "learner@example.com",
            TimeZoneId = "Europe/Istanbul",
            DailyXpGoal = 20
        };
        db.Users.Add(user);
        db.UserStreaks.Add(new UserStreak { UserId = user.Id });
        await db.SaveChangesAsync();

        var service = new StreakService(db);

        await service.ApplyLearningActivityAsync(user.Id, DateTimeOffset.Parse("2026-04-27T21:30:00Z"));
        var streak = await service.ApplyLearningActivityAsync(user.Id, DateTimeOffset.Parse("2026-04-28T21:15:00Z"));

        Assert.Equal(2, streak.CurrentStreakDays);
        Assert.Equal(new DateOnly(2026, 4, 29), streak.LastCompletedLocalDate);
    }

    [Fact]
    public void League_result_promotes_top_ten_demotes_bottom_five_and_protects_new_users()
    {
        var seasonEndsAt = DateTimeOffset.Parse("2026-04-28T00:00:00Z");
        var participants = Enumerable.Range(1, 30)
            .Select(index => new LeagueParticipant
            {
                UserId = Guid.NewGuid(),
                DisplayName = $"Learner {index}",
                LeagueTier = LeagueTier.Bronze,
                WeeklyXp = 310 - (index * 10),
                JoinedAtUtc = index == 30 ? seasonEndsAt.AddDays(-7) : seasonEndsAt.AddDays(-30)
            })
            .ToList();

        var results = LeagueService.CalculateSeasonResults(participants, seasonEndsAt);

        Assert.Equal(LeagueMovement.Promote, results.Single(x => x.Rank == 1).Movement);
        Assert.Equal(LeagueMovement.Promote, results.Single(x => x.Rank == 10).Movement);
        Assert.Equal(LeagueMovement.Stay, results.Single(x => x.Rank == 11).Movement);
        Assert.Equal(LeagueMovement.Demote, results.Single(x => x.Rank == 26).Movement);
        Assert.Equal(LeagueMovement.Stay, results.Single(x => x.Rank == 30).Movement);
    }

    [Fact]
    public async Task Experiment_assignment_is_deterministic_per_user_and_experiment()
    {
        await using var db = TestDb.Create();
        var experiment = new Experiment
        {
            Id = Guid.NewGuid(),
            Key = "lesson_cta_copy",
            Name = "Lesson CTA copy",
            IsActive = true,
            PrimaryMetric = "first_lesson_completed",
            GuardrailMetric = "bounce_rate",
            Variants =
            [
                new ExperimentVariant { Id = Guid.NewGuid(), Key = "start_path", Weight = 1 },
                new ExperimentVariant { Id = Guid.NewGuid(), Key = "ai_byte", Weight = 1 },
                new ExperimentVariant { Id = Guid.NewGuid(), Key = "mini_lesson", Weight = 1 }
            ]
        };
        db.Experiments.Add(experiment);
        await db.SaveChangesAsync();

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new ExperimentAssignmentService(db);

        var first = await service.AssignAsync(userId, "lesson_cta_copy");
        var second = await service.AssignAsync(userId, "lesson_cta_copy");

        Assert.Equal(first.VariantKey, second.VariantKey);
        Assert.Equal(1, await db.ExperimentAssignments.CountAsync(x => x.UserId == userId));
    }

    [Fact]
    public async Task Lesson_completion_requires_exercise_before_xp_is_granted()
    {
        await using var db = TestDb.Create();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "learner@example.com",
            Email = "learner@example.com",
            TimeZoneId = "Europe/Istanbul",
            DailyXpGoal = 20
        };
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            Slug = "beginner-ai-byte",
            Title = "AI Byte",
            LearningObjective = "Explain one concept",
            XpReward = 10,
            DurationMinutes = 4,
            Exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Type = ExerciseType.ShortAnswer,
                Prompt = "Explain it in one sentence.",
                AutoGradable = false,
                RequiresAiFeedback = true
            }
        };
        db.Users.Add(user);
        db.Lessons.Add(lesson);
        db.UserStreaks.Add(new UserStreak { UserId = user.Id });
        await db.SaveChangesAsync();

        var service = new LessonCompletionService(db, new XpService(db), new StreakService(db));

        var blocked = await service.CompleteAsync(user.Id, lesson.Slug, exerciseSubmitted: false, DateTimeOffset.UtcNow);
        var completed = await service.CompleteAsync(user.Id, lesson.Slug, exerciseSubmitted: true, DateTimeOffset.UtcNow);

        Assert.False(blocked.Succeeded);
        Assert.Equal("Mini alıştırma tamamlanmadan ders bitirilemez.", blocked.Error);
        Assert.True(completed.Succeeded);
        Assert.Equal(10, await db.XpTransactions.Where(x => x.UserId == user.Id).SumAsync(x => x.Amount));
    }
}

