using System.Net.Http.Json;

namespace AiEducation.Api.Tests;

public sealed class MvpFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Register_onboard_complete_lesson_updates_xp_streak_quest_and_experiment()
    {
        var client = factory.CreateClient();
        var token = await ApiFactory.RegisterAndGetTokenAsync(client);
        ApiFactory.Authorize(client, token);

        var onboarding = await client.PostAsJsonAsync("/api/v1/onboarding", new
        {
            dailyXpGoal = 20,
            learningPathSlug = "beginner"
        });
        onboarding.EnsureSuccessStatusCode();

        var start = await client.PostAsJsonAsync("/api/v1/lessons/beginner-ai-okuryazarligi-01/start", new { });
        start.EnsureSuccessStatusCode();

        var complete = await client.PostAsJsonAsync("/api/v1/lessons/beginner-ai-okuryazarligi-01/complete", new
        {
            exerciseSubmitted = true,
            answer = "Sadece sabit kurallı bir e-posta yönlendirme otomasyonu AI değildir."
        });
        complete.EnsureSuccessStatusCode();

        var gamification = await client.GetFromJsonAsync<GamificationPayload>("/api/v1/gamification/me");
        var assignments = await client.GetFromJsonAsync<ExperimentPayload[]>("/api/v1/experiments/assignments");

        Assert.Equal(10, gamification!.TotalXp);
        Assert.Equal(1, gamification.CurrentStreakDays);
        Assert.Contains(gamification.DailyQuests, quest => quest.Slug == "daily-micro-lesson" && quest.Completed);
        Assert.Contains(assignments!, assignment => assignment.ExperimentKey == "lesson_cta_copy" && assignment.VariantKey.Length > 0);
    }

    private sealed record GamificationPayload(int TotalXp, int CurrentStreakDays, QuestPayload[] DailyQuests);

    private sealed record QuestPayload(string Slug, bool Completed);

    private sealed record ExperimentPayload(string ExperimentKey, string VariantKey);
}
