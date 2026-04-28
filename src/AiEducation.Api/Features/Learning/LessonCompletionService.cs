using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.Gamification;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Learning;

public sealed record LessonCompletionResult(bool Succeeded, string? Error = null, int XpGranted = 0);

public sealed class LessonCompletionService(
    AppDbContext db,
    XpService xpService,
    StreakService streakService,
    QuestProgressService? questProgressService = null,
    AnalyticsService? analyticsService = null)
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
            .SingleOrDefaultAsync(x => x.Slug == lessonSlug && x.Status == Models.ContentStatus.Published && !x.IsArchived, cancellationToken);
        if (lesson is null)
        {
            return new LessonCompletionResult(false, "Ders bulunamadı.");
        }

        var hasRequiredSubmission = lesson.Exercise is null || await db.UserExerciseSubmissions.AnyAsync(
            x => x.UserId == userId
                && x.LessonId == lesson.Id
                && x.ExerciseId == lesson.Exercise.Id
                && x.PassedQualityGate,
            cancellationToken);

        if (!hasRequiredSubmission)
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

        var xpTransaction = await xpService.GrantXpAsync(
            userId,
            XpEvents.MicroLessonCompleted,
            lesson.XpReward,
            "Lesson",
            lesson.Id,
            passedQualityGate: true,
            completedAtUtc,
            cancellationToken);

        await streakService.ApplyLearningActivityAsync(userId, completedAtUtc, cancellationToken);
        if (questProgressService is not null)
        {
            await questProgressService.MarkCompletedAsync(userId, "daily-micro-lesson", completedAtUtc, cancellationToken);
        }

        if (analyticsService is not null)
        {
            await analyticsService.TrackAsync(userId, AnalyticsEvents.LessonCompleted, new
            {
                lesson.Slug,
                lesson.Id,
                exerciseSubmitted
            }, completedAtUtc, cancellationToken);
        }

        return new LessonCompletionResult(true, XpGranted: xpTransaction.Amount);
    }
}
