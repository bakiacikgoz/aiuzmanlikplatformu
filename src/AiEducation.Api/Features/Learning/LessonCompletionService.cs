using AiEducation.Api.Data;
using AiEducation.Api.Features.Gamification;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Learning;

public sealed record LessonCompletionResult(bool Succeeded, string? Error = null, int XpGranted = 0);

public sealed class LessonCompletionService(AppDbContext db, XpService xpService, StreakService streakService)
{
    public async Task<LessonCompletionResult> CompleteAsync(
        Guid userId,
        string lessonSlug,
        bool exerciseSubmitted,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var lesson = await db.Lessons
            .Include(x => x.Exercise)
            .SingleOrDefaultAsync(x => x.Slug == lessonSlug, cancellationToken);
        if (lesson is null)
        {
            return new LessonCompletionResult(false, "Ders bulunamadı.");
        }

        if (!exerciseSubmitted)
        {
            return new LessonCompletionResult(false, "Mini alıştırma tamamlanmadan ders bitirilemez.");
        }

        var progress = await db.UserLessonProgresses
            .SingleOrDefaultAsync(x => x.UserId == userId && x.LessonId == lesson.Id, cancellationToken);

        if (progress is null)
        {
            progress = new Models.UserLessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lesson.Id,
                StartedAtUtc = completedAtUtc
            };
            db.UserLessonProgresses.Add(progress);
        }

        if (progress.CompletedAtUtc is not null)
        {
            return new LessonCompletionResult(true, XpGranted: 0);
        }

        progress.CompletedAtUtc = completedAtUtc;
        progress.ExerciseSubmitted = true;
        progress.ScorePercent = Math.Max(progress.ScorePercent ?? 0, lesson.PassingScorePercent);
        await db.SaveChangesAsync(cancellationToken);

        await xpService.GrantXpAsync(
            userId,
            XpEvents.MicroLessonCompleted,
            lesson.XpReward,
            "Lesson",
            lesson.Id,
            passedQualityGate: true,
            completedAtUtc,
            cancellationToken);

        await streakService.ApplyLearningActivityAsync(userId, completedAtUtc, cancellationToken);
        await CompleteDailyQuestAsync(userId, completedAtUtc, cancellationToken);

        return new LessonCompletionResult(true, XpGranted: lesson.XpReward);
    }

    private async Task CompleteDailyQuestAsync(Guid userId, DateTimeOffset completedAtUtc, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken);
        var localDate = StreakService.ToLocalDate(completedAtUtc, user?.TimeZoneId ?? "Europe/Istanbul");
        var quest = await db.Quests.FirstOrDefaultAsync(x => x.Slug == "daily-micro-lesson", cancellationToken);
        if (quest is null)
        {
            return;
        }

        var userQuest = await db.UserQuests.SingleOrDefaultAsync(
            x => x.UserId == userId && x.QuestId == quest.Id && x.LocalDate == localDate,
            cancellationToken);
        if (userQuest is null)
        {
            userQuest = new Models.UserQuest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                QuestId = quest.Id,
                LocalDate = localDate
            };
            db.UserQuests.Add(userQuest);
        }

        userQuest.Completed = true;
        await db.SaveChangesAsync(cancellationToken);
    }
}
