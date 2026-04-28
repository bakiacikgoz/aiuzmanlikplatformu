using System.Net;
using System.Net.Http.Json;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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

        var blockedComplete = await client.PostAsJsonAsync("/api/v1/lessons/beginner-ai-okuryazarligi-01/complete", new
        {
            exerciseSubmitted = true,
            answer = "Sadece sabit kurallı bir e-posta yönlendirme otomasyonu AI değildir."
        });
        Assert.Equal(HttpStatusCode.BadRequest, blockedComplete.StatusCode);

        var exercise = await client.PostAsJsonAsync("/api/v1/exercises/dddddddd-dddd-dddd-dddd-dddddddddddd/submit", new
        {
            answer = "Sadece sabit kurallı bir e-posta yönlendirme otomasyonu AI değildir."
        });
        exercise.EnsureSuccessStatusCode();

        var complete = await client.PostAsJsonAsync("/api/v1/lessons/beginner-ai-okuryazarligi-01/complete", new
        {
            exerciseSubmitted = false,
            answer = ""
        });
        complete.EnsureSuccessStatusCode();

        var gamification = await client.GetFromJsonAsync<GamificationPayload>("/api/v1/gamification/me");
        var assignments = await client.GetFromJsonAsync<ExperimentPayload[]>("/api/v1/experiments/assignments");

        Assert.Equal(25, gamification!.TotalXp);
        Assert.Equal(1, gamification.CurrentStreakDays);
        Assert.Contains(gamification.DailyQuests, quest => quest.Slug == "daily-micro-lesson" && quest.Completed);
        Assert.Contains(gamification.DailyQuests, quest => quest.Slug == "daily-practice" && quest.Completed);
        Assert.Contains(assignments!, assignment => assignment.ExperimentKey == "lesson_cta_copy" && assignment.VariantKey.Length > 0);
    }

    [Fact]
    public async Task Quiz_project_analytics_notification_ai_and_admin_security_paths_work()
    {
        var client = factory.CreateClient();
        var token = await ApiFactory.RegisterAndGetTokenAsync(client, "learner2@example.com");
        ApiFactory.Authorize(client, token);

        var quiz = await client.PostAsJsonAsync("/api/v1/quizzes/beginner-ai-okuryazarligi-checkpoint/attempts", new
        {
            answers = new Dictionary<string, string>
            {
                ["11111111-2222-3333-4444-555555555555"] = "aaaaaaaa-1111-1111-1111-111111111111",
                ["22222222-3333-4444-5555-666666666666"] = "bbbbbbbb-2222-2222-2222-222222222222"
            }
        });
        quiz.EnsureSuccessStatusCode();
        var quizPayload = await quiz.Content.ReadFromJsonAsync<QuizPayload>();
        Assert.Equal(50, quizPayload!.ScorePercent);
        Assert.False(quizPayload.Passed);
        Assert.Equal(0, quizPayload.XpGranted);

        var project = await client.PostAsJsonAsync("/api/v1/projects/first-ai-portfolio/submissions", new
        {
            repositoryUrl = "https://github.com/example/first-ai-portfolio",
            demoUrl = "",
            notes = "İlk geçerli teslim"
        });
        project.EnsureSuccessStatusCode();
        var portfolio = await client.GetFromJsonAsync<PortfolioPayload[]>("/api/v1/portfolio/me");
        Assert.Contains(portfolio!, item => item.EvidenceUrl == "https://github.com/example/first-ai-portfolio");

        var notifications = await client.GetFromJsonAsync<NotificationPayload[]>("/api/v1/notifications");
        Assert.NotEmpty(notifications!);
        var preferences = await client.GetFromJsonAsync<NotificationPreferencesPayload>("/api/v1/notification-preferences");
        Assert.True(preferences!.MorningReminderEnabled);

        var aiOne = await client.PostAsJsonAsync("/api/v1/ai/chat", new { message = "Bu konuyu kısa anlat." });
        aiOne.EnsureSuccessStatusCode();
        var aiTwo = await client.PostAsJsonAsync("/api/v1/ai/chat", new { message = "Bir örnek daha ver." });
        aiTwo.EnsureSuccessStatusCode();
        var aiThree = await client.PostAsJsonAsync("/api/v1/ai/chat", new { message = "Limit testi." });
        Assert.Equal((HttpStatusCode)429, aiThree.StatusCode);

        var learnerAdminAccess = await client.GetAsync("/api/v1/admin/content");
        Assert.Equal(HttpStatusCode.Forbidden, learnerAdminAccess.StatusCode);

        var adminClient = factory.CreateClient();
        var adminToken = await ApiFactory.RegisterAndGetTokenAsync(adminClient, "admin-test@example.com");
        await PromoteToAdminAsync("admin-test@example.com");
        adminToken = await LoginAndGetTokenAsync(adminClient, "admin-test@example.com");
        ApiFactory.Authorize(adminClient, adminToken);
        var adminContent = await adminClient.GetAsync("/api/v1/admin/content");
        adminContent.EnsureSuccessStatusCode();
    }

    private async Task PromoteToAdminAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
        }

        var user = await userManager.FindByEmailAsync(email) ?? throw new InvalidOperationException("Admin test user not found.");
        await userManager.AddToRoleAsync(user, "Admin");
    }

    private static async Task<string> LoginAndGetTokenAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "Passw0rd!"
        });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthPayload>();
        return payload!.AccessToken;
    }

    private sealed record GamificationPayload(int TotalXp, int CurrentStreakDays, QuestPayload[] DailyQuests);

    private sealed record QuestPayload(string Slug, bool Completed);

    private sealed record ExperimentPayload(string ExperimentKey, string VariantKey);

    private sealed record QuizPayload(bool Passed, int ScorePercent, int XpGranted);

    private sealed record PortfolioPayload(string Title, string EvidenceUrl);

    private sealed record NotificationPayload(Guid Id, string Status);

    private sealed record NotificationPreferencesPayload(bool MorningReminderEnabled);

    private sealed record AuthPayload(string AccessToken);
}
