using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiEducation.Api.Tests;

public sealed class QuizEditorTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Admin_can_create_quiz_questions_and_student_score_is_calculated_by_backend()
    {
        var admin = factory.CreateClient();
        ApiFactory.Authorize(admin, await factory.RegisterAdminAndGetTokenAsync(admin, "admin-quiz@example.com"));

        var lessons = await admin.GetFromJsonAsync<AdminLessonPayload[]>("/api/v1/admin/lessons");
        var quizLesson = lessons!.Single(x => x.Slug == "beginner-ai-okuryazarligi-checkpoint");

        var invalid = await admin.PostAsJsonAsync($"/api/v1/admin/lessons/{quizLesson.Id}/quiz-questions", new
        {
            questionText = "Geçersiz soru",
            explanation = "Birden fazla doğru seçenek olmamalı.",
            sortOrder = 10,
            options = new[]
            {
                new { text = "A", isCorrect = true, sortOrder = 1 },
                new { text = "B", isCorrect = true, sortOrder = 2 }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var createdQuestions = new List<CreatedQuestionPayload>();
        for (var index = 1; index <= 3; index++)
        {
            var response = await admin.PostAsJsonAsync($"/api/v1/admin/lessons/{quizLesson.Id}/quiz-questions", new
            {
                questionText = $"Admin quiz sorusu {index}",
                explanation = $"Açıklama {index}",
                sortOrder = index + 10,
                options = new[]
                {
                    new { text = "Doğru cevap", isCorrect = true, sortOrder = 1 },
                    new { text = "Yanlış cevap 1", isCorrect = false, sortOrder = 2 },
                    new { text = "Yanlış cevap 2", isCorrect = false, sortOrder = 3 },
                    new { text = "Yanlış cevap 3", isCorrect = false, sortOrder = 4 }
                }
            });
            response.EnsureSuccessStatusCode();
            createdQuestions.Add((await response.Content.ReadFromJsonAsync<CreatedQuestionPayload>())!);
        }

        var student = factory.CreateClient();
        ApiFactory.Authorize(student, await ApiFactory.RegisterAndGetTokenAsync(student, "student-quiz@example.com"));
        var lessonJson = await student.GetStringAsync("/api/v1/lessons/beginner-ai-okuryazarligi-checkpoint");
        Assert.DoesNotContain("isCorrect", lessonJson, StringComparison.OrdinalIgnoreCase);

        using var lessonDoc = JsonDocument.Parse(lessonJson);
        var questions = lessonDoc.RootElement.GetProperty("quizQuestions").EnumerateArray().ToArray();
        var answers = questions.ToDictionary(
            question => question.GetProperty("id").GetString()!,
            question => question.GetProperty("options").EnumerateArray().First().GetProperty("id").GetString()!);

        var attempt = await student.PostAsJsonAsync("/api/v1/quizzes/beginner-ai-okuryazarligi-checkpoint/attempts", new
        {
            scorePercent = 0,
            wrongAnswers = Array.Empty<string>(),
            answers
        });
        attempt.EnsureSuccessStatusCode();
        var attemptPayload = await attempt.Content.ReadFromJsonAsync<AttemptPayload>();
        Assert.Equal(100, attemptPayload!.ScorePercent);
        Assert.True(attemptPayload.Passed);
        Assert.True(attemptPayload.XpGranted > 0);
    }

    private sealed record AdminLessonPayload(Guid Id, string Slug);

    private sealed record CreatedQuestionPayload(Guid Id);

    private sealed record AttemptPayload(int ScorePercent, bool Passed, int XpGranted);
}
