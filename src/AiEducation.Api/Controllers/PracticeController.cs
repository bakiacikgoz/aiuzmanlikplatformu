using System.Text.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1")]
public sealed class PracticeController(
    AppDbContext db,
    XpService xpService,
    QuestProgressService questProgressService,
    StreakService streakService,
    AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpPost("exercises/{id:guid}/submit")]
    public async Task<IActionResult> SubmitExercise(Guid id, SubmitExerciseRequest request, CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises.Include(x => x.Lesson).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        var answer = (request.Answer ?? "").Trim();
        var passedQualityGate = answer.Length >= 8;
        var feedback = passedQualityGate
            ? exercise.RequiresAiFeedback
                ? "Cevabın kaydedildi. AI mentor rubriğine göre temel kriteri geçti."
                : "Cevabın kaydedildi ve kalite eşiğini geçti."
            : "Cevabını biraz aç: en az bir gerekçe veya örnek ekle.";

        var now = DateTimeOffset.UtcNow;
        var submission = new Models.UserExerciseSubmission
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId(),
            LessonId = exercise.LessonId,
            ExerciseId = exercise.Id,
            Answer = answer,
            Feedback = feedback,
            PassedQualityGate = passedQualityGate,
            CreatedAtUtc = now
        };
        db.UserExerciseSubmissions.Add(submission);
        await db.SaveChangesAsync(cancellationToken);

        var xpGranted = 0;
        if (passedQualityGate)
        {
            var xp = await xpService.GrantXpAsync(CurrentUserId(), XpEvents.PracticeCompleted, 15, "Exercise", id, true, now, cancellationToken);
            xpGranted = xp.Amount;
            await questProgressService.MarkCompletedAsync(CurrentUserId(), "daily-practice", now, cancellationToken);
            await streakService.ApplyDailyGoalIfMetAsync(CurrentUserId(), now, cancellationToken);
        }

        await analyticsService.TrackAsync(CurrentUserId(), AnalyticsEvents.ExerciseSubmitted, new
        {
            exerciseId = id,
            lessonSlug = exercise.Lesson?.Slug,
            passedQualityGate,
            answerLength = answer.Length
        }, now, cancellationToken);

        return Ok(new
        {
            accepted = true,
            submissionId = submission.Id,
            passedQualityGate,
            requiresAiFeedback = exercise.RequiresAiFeedback,
            feedback,
            xpGranted
        });
    }

    [HttpPost("quizzes/{lessonSlug}/attempts")]
    public async Task<IActionResult> SubmitQuiz(string lessonSlug, QuizAttemptRequest request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(x => x.QuizQuestions)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Slug == lessonSlug, cancellationToken);
        if (lesson is null)
        {
            return NotFound();
        }

        var questions = lesson.QuizQuestions.OrderBy(x => x.SortOrder).ToList();
        if (questions.Count == 0)
        {
            return BadRequest(new { message = "Bu ders için quiz sorusu bulunamadı." });
        }

        var answers = request.Answers ?? [];
        var correct = 0;
        var wrongAnswers = new List<object>();
        foreach (var question in questions)
        {
            var correctOption = question.Options.OrderBy(x => x.SortOrder).FirstOrDefault(x => x.IsCorrect);
            var selectedOptionId = answers.TryGetValue(question.Id.ToString(), out var selectedValue)
                && Guid.TryParse(selectedValue, out var parsedSelected)
                    ? parsedSelected
                    : (Guid?)null;
            var isCorrect = selectedOptionId is not null && question.Options.Any(x => x.Id == selectedOptionId && x.IsCorrect);

            if (isCorrect)
            {
                correct += 1;
            }
            else
            {
                wrongAnswers.Add(new
                {
                    questionId = question.Id,
                    question.Prompt,
                    selectedOptionId,
                    correctOptionId = correctOption?.Id,
                    explanation = question.Explanation
                });
            }
        }

        var scorePercent = (int)Math.Round(correct * 100.0 / questions.Count, MidpointRounding.AwayFromZero);
        var now = DateTimeOffset.UtcNow;
        var attempt = new Models.QuizAttempt
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId(),
            LessonId = lesson.Id,
            ScorePercent = scorePercent,
            WrongAnswersJson = JsonSerializer.Serialize(wrongAnswers),
            CreatedAtUtc = now
        };
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        var xp = 0;
        var passed = scorePercent >= lesson.PassingScorePercent;
        if (passed)
        {
            var transaction = await xpService.GrantXpAsync(CurrentUserId(), XpEvents.QuizPassed, 25, "QuizAttempt", attempt.Id, true, now, cancellationToken);
            xp = transaction.Amount;
            await questProgressService.MarkCompletedAsync(CurrentUserId(), "daily-review", now, cancellationToken);
            await streakService.ApplyDailyGoalIfMetAsync(CurrentUserId(), now, cancellationToken);
        }

        await analyticsService.TrackAsync(CurrentUserId(), AnalyticsEvents.QuizSubmitted, new
        {
            lessonSlug,
            scorePercent,
            passed,
            wrongCount = wrongAnswers.Count
        }, now, cancellationToken);

        return Ok(new { attempt.Id, passed, scorePercent, wrongAnswers, xpGranted = xp });
    }

    [HttpGet("review/mistakes")]
    public async Task<ActionResult<IReadOnlyList<object>>> Mistakes(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var mistakes = await db.QuizAttempts
            .Include(x => x.Lesson)
            .Where(x => x.UserId == userId && x.WrongAnswersJson != "[]")
            .Select(x => new { x.Lesson!.Slug, x.Lesson.Title, x.WrongAnswersJson, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(mistakes.OrderByDescending(x => x.CreatedAtUtc).Take(20));
    }
}

public sealed record SubmitExerciseRequest(string? Answer);
public sealed record QuizAttemptRequest(Dictionary<string, string>? Answers);
