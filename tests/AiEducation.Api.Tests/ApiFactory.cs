using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiEducation.Api.Data;
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

    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "learner@example.com",
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
        var quest = new Quest
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Slug = "daily-micro-lesson",
            Title = "Bir AI Byte tamamla",
            Cadence = QuestCadence.Daily,
            RewardXp = 10,
            RewardGems = 5
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
        db.Lessons.Add(lesson);
        db.Quests.Add(quest);
        db.Experiments.Add(experiment);
        db.SaveChanges();
    }

    private sealed record AuthPayload(string AccessToken);
}
