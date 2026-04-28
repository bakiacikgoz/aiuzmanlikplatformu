using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AiEducation.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }

    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client, string email = "learner@example.com")
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Passw0rd!",
            displayName = "Learner",
            timeZoneId = "Europe/Istanbul"
        });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthPayload>();
        return payload!.AccessToken;
    }

    public static void Authorize(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static void Seed(AppDbContext db)
    {
        var path = new LearningPath
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Slug = "beginner",
            Title = "Başlangıç AI Uzmanı",
            Level = "beginner",
            SortOrder = 1
        };
        var unit = new Unit
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            LearningPathId = path.Id,
            Slug = "ai-okuryazarligi",
            Title = "AI Okuryazarlığı",
            SortOrder = 1
        };
        var lesson = new Lesson
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            UnitId = unit.Id,
            Slug = "beginner-ai-okuryazarligi-01",
            Title = "Yapay zeka nedir, ne değildir?",
            LearningObjective = "AI kavramını tek cümlede açıklamak.",
            MiniExplanation = "Yapay zeka, belirli örüntülerden tahmin veya karar üreten sistemler için kullanılan geniş bir kavramdır.",
            TinyExample = "Bir e-posta filtresinin spam olasılığını tahmin etmesi küçük bir AI örneğidir.",
            XpReward = 10,
            DurationMinutes = 4,
            SortOrder = 1,
            Exercise = new Exercise
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Type = ExerciseType.ShortAnswer,
                Prompt = "AI olmayan bir otomasyon örneği yaz.",
                RequiresAiFeedback = true
            }
        };
        var quizLesson = new Lesson
        {
            Id = Guid.Parse("cfcfcfcf-cfcf-cfcf-cfcf-cfcfcfcfcfcf"),
            UnitId = unit.Id,
            Slug = "beginner-ai-okuryazarligi-checkpoint",
            Title = "AI Okuryazarlığı Checkpoint",
            LessonType = "checkpoint_quiz",
            LearningObjective = "AI temel kavramlarını ölçmek.",
            XpReward = 25,
            DurationMinutes = 5,
            QuestionCount = 2,
            PassingScorePercent = 70,
            SortOrder = 99,
            QuizQuestions =
            [
                new QuizQuestion
                {
                    Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    Prompt = "AI olmayan örnek hangisidir?",
                    Explanation = "Sabit kurallı otomasyon AI değildir.",
                    SortOrder = 1,
                    Options =
                    [
                        new QuizOption { Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"), Text = "Sabit kuralla e-posta taşıma", IsCorrect = true, SortOrder = 1 },
                        new QuizOption { Id = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222"), Text = "Spam olasılığı tahmini", IsCorrect = false, SortOrder = 2 }
                    ]
                },
                new QuizQuestion
                {
                    Id = Guid.Parse("22222222-3333-4444-5555-666666666666"),
                    Prompt = "AI mentor çağrısı nereden yapılır?",
                    Explanation = "AI sağlayıcı backend abstraction arkasındadır.",
                    SortOrder = 2,
                    Options =
                    [
                        new QuizOption { Id = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"), Text = "Backend üzerinden", IsCorrect = true, SortOrder = 1 },
                        new QuizOption { Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"), Text = "Frontend API key ile", IsCorrect = false, SortOrder = 2 }
                    ]
                }
            ]
        };
        var quest = new Quest
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Slug = "daily-micro-lesson",
            Title = "Bir AI Byte tamamla",
            Cadence = QuestCadence.Daily,
            RewardXp = 10,
            RewardGems = 5
        };
        var practiceQuest = new Quest
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1"),
            Slug = "daily-practice",
            Title = "Bir pratik cevabı gönder",
            Cadence = QuestCadence.Daily,
            RewardXp = 0,
            RewardGems = 3
        };
        var reviewQuest = new Quest
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2"),
            Slug = "daily-review",
            Title = "Bir quiz çöz",
            Cadence = QuestCadence.Daily,
            RewardXp = 0,
            RewardGems = 3
        };
        var experiment = new Experiment
        {
            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Key = "lesson_cta_copy",
            Name = "Lesson CTA copy",
            IsActive = true,
            PrimaryMetric = "first_lesson_completed",
            GuardrailMetric = "bounce_rate",
            Variants =
            [
                new ExperimentVariant { Id = Guid.NewGuid(), Key = "start_path", Weight = 1 },
                new ExperimentVariant { Id = Guid.NewGuid(), Key = "ai_byte", Weight = 1 }
            ]
        };

        db.LearningPaths.Add(path);
        db.Units.Add(unit);
        var project = new Project
        {
            Id = Guid.Parse("12121212-1212-1212-1212-121212121212"),
            Slug = "first-ai-portfolio",
            Level = "beginner",
            Title = "İlk AI Portföy Projesi",
            Duration = "1 hafta",
            XpReward = 120,
            DeliverablesJson = "[\"repo\"]"
        };
        project.RubricCriteria.Add(new ProjectRubricCriterion
        {
            Id = Guid.Parse("13131313-1313-1313-1313-131313131313"),
            Criterion = "Çalışan demo",
            Points = 40
        });

        db.Lessons.AddRange(lesson, quizLesson);
        db.Quests.AddRange(quest, practiceQuest, reviewQuest);
        db.Experiments.Add(experiment);
        db.Projects.Add(project);
        db.XpRules.AddRange(
            new XpRule { Id = Guid.NewGuid(), Event = XpEvents.MicroLessonCompleted, Xp = 10, DailyCap = 60, QualityGate = "lesson_submission" },
            new XpRule { Id = Guid.NewGuid(), Event = XpEvents.PracticeCompleted, Xp = 15, DailyCap = 60, QualityGate = "non_blank_answer" },
            new XpRule { Id = Guid.NewGuid(), Event = XpEvents.QuizPassed, Xp = 25, DailyCap = 100, QualityGate = "score_70" },
            new XpRule { Id = Guid.NewGuid(), Event = XpEvents.ProjectSubmitted, Xp = 120, DailyCap = 200, QualityGate = "valid_project_link" });
        db.SubscriptionPlans.Add(new SubscriptionPlan { Id = Guid.Parse("14141414-1414-1414-1414-141414141414"), Slug = "free", Name = "Free", MonthlyAiMessageLimit = 2 });
        db.NotificationTemplates.Add(new NotificationTemplate
        {
            Id = Guid.Parse("15151515-1515-1515-1515-151515151515"),
            Slug = "streak_reminder",
            Channel = "web",
            Title = "Serini koru",
            Body = "Bugünkü AI Byte hazır."
        });
        db.SaveChanges();
    }

    private sealed record AuthPayload(string AccessToken);
}
