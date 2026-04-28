using System.Text.Json;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Data;

public sealed class SeedDataImporter(AppDbContext db, IWebHostEnvironment environment, ILogger<SeedDataImporter> logger)
{
    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        var seedPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "ai_egitim_platformu_gamified_seed_v2.json"));
        if (!File.Exists(seedPath))
        {
            logger.LogWarning("Seed file not found at {SeedPath}", seedPath);
            return;
        }

        await using var stream = File.OpenRead(seedPath);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        await ImportResourcesAsync(root, cancellationToken);
        await ImportLearningPathsAsync(root, cancellationToken);
        await ImportGamificationAsync(root, cancellationToken);
        await ImportProjectsAsync(root, cancellationToken);
        await ImportNotificationsAsync(root, cancellationToken);
        await ImportExperimentsAsync(root, cancellationToken);
        await ImportSubscriptionPlansAsync(cancellationToken);
    }

    private async Task ImportResourcesAsync(JsonElement root, CancellationToken cancellationToken)
    {
        foreach (var item in root.GetProperty("resources").EnumerateArray())
        {
            var slug = Text(item, "slug");
            if (await db.Resources.AnyAsync(x => x.Slug == slug, cancellationToken))
            {
                continue;
            }

            db.Resources.Add(new Resource
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                Title = Text(item, "title"),
                Url = Text(item, "url"),
                Type = Text(item, "type", "resource"),
                Summary = Text(item, "summary", Text(item, "topic", ""))
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportLearningPathsAsync(JsonElement root, CancellationToken cancellationToken)
    {
        foreach (var pathElement in root.GetProperty("learningPaths").EnumerateArray())
        {
            var pathSlug = Text(pathElement, "slug");
            var path = await db.LearningPaths.SingleOrDefaultAsync(x => x.Slug == pathSlug, cancellationToken);
            if (path is null)
            {
                path = new LearningPath
                {
                    Id = Guid.NewGuid(),
                    Slug = pathSlug,
                    Title = Text(pathElement, "title"),
                    Level = pathSlug,
                    Description = Text(pathElement, "target", Text(pathElement, "duration", "")),
                    SortOrder = Int(pathElement, "order", 0)
                };
                db.LearningPaths.Add(path);
                await db.SaveChangesAsync(cancellationToken);
            }

            foreach (var unitElement in pathElement.GetProperty("units").EnumerateArray())
            {
                var unitSlug = Text(unitElement, "slug");
                var unit = await db.Units.SingleOrDefaultAsync(x => x.Slug == unitSlug, cancellationToken);
                if (unit is null)
                {
                    unit = new Unit
                    {
                        Id = Guid.NewGuid(),
                        LearningPathId = path.Id,
                        Slug = unitSlug,
                        Title = Text(unitElement, "title"),
                        SortOrder = Int(unitElement, "order", 0)
                    };
                    db.Units.Add(unit);
                    await db.SaveChangesAsync(cancellationToken);
                }

                foreach (var lessonElement in unitElement.GetProperty("lessons").EnumerateArray())
                {
                    await ImportLessonAsync(unit, lessonElement, cancellationToken);
                }
            }
        }
    }

    private async Task ImportLessonAsync(Unit unit, JsonElement lessonElement, CancellationToken cancellationToken)
    {
        var lessonSlug = Text(lessonElement, "slug");
        if (await db.Lessons.AnyAsync(x => x.Slug == lessonSlug, cancellationToken))
        {
            return;
        }

        var title = Text(lessonElement, "title");
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            Slug = lessonSlug,
            Title = title,
            LessonType = Text(lessonElement, "lessonType", "micro_lesson"),
            DurationMinutes = Int(lessonElement, "durationMinutes", 4),
            XpReward = Int(lessonElement, "xpReward", 10),
            DailyEligible = Bool(lessonElement, "dailyEligible", true),
            Difficulty = Text(lessonElement, "difficulty", "easy"),
            LearningObjective = Text(lessonElement, "learningObjective", $"Kullanıcı {title} konusunu tek küçük beceri olarak uygular."),
            MiniExplanation = $"{title} konusunu tek kavram üzerinden öğren. Gerektiğinde AI mentordan farklı bir örnek iste.",
            TinyExample = $"Örnek: {title} gerçek bir AI öğrenme görevinde küçük ve ölçülebilir bir davranışa dönüştürülür.",
            CompletionCriteriaJson = JsonSerializer.Serialize(StringArray(lessonElement, "completionCriteria")),
            PassingScorePercent = Int(lessonElement, "passingScorePercent", 70),
            QuestionCount = Int(lessonElement, "questionCount", 0),
            SortOrder = LessonOrder(lessonSlug)
        };

        if (lessonElement.TryGetProperty("exercise", out var exerciseElement) && exerciseElement.ValueKind == JsonValueKind.Object)
        {
            lesson.Exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Type = MapExerciseType(Text(exerciseElement, "type", "short_answer")),
                Prompt = Text(exerciseElement, "prompt", $"{title} için kısa bir cevap yaz."),
                AutoGradable = Bool(exerciseElement, "autoGradable", false),
                RequiresAiFeedback = Bool(exerciseElement, "requiresAiFeedback", true)
            };
        }

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var resourceSlug in StringArray(lessonElement, "resourceSlugs"))
        {
            var resource = await db.Resources.SingleOrDefaultAsync(x => x.Slug == resourceSlug, cancellationToken);
            if (resource is not null)
            {
                db.LessonResources.Add(new LessonResource { LessonId = lesson.Id, ResourceId = resource.Id });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportGamificationAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var config = root.GetProperty("gamificationConfig");
        foreach (var ruleElement in config.GetProperty("xpRules").EnumerateArray())
        {
            var eventName = Text(ruleElement, "event");
            if (!await db.XpRules.AnyAsync(x => x.Event == eventName, cancellationToken))
            {
                db.XpRules.Add(new XpRule
                {
                    Id = Guid.NewGuid(),
                    Event = eventName,
                    Xp = Int(ruleElement, "xp", 0),
                    DailyCap = Int(ruleElement, "dailyCap", 0),
                    QualityGate = Text(ruleElement, "qualityGate", "")
                });
            }
        }

        foreach (var badgeElement in config.GetProperty("badges").EnumerateArray())
        {
            var slug = Text(badgeElement, "slug");
            if (!await db.Badges.AnyAsync(x => x.Slug == slug, cancellationToken))
            {
                db.Badges.Add(new Badge { Id = Guid.NewGuid(), Slug = slug, Title = Text(badgeElement, "title"), Condition = Text(badgeElement, "condition") });
            }
        }

        await ImportQuestsAsync(config.GetProperty("dailyQuests"), QuestCadence.Daily, cancellationToken);
        await ImportQuestsAsync(config.GetProperty("weeklyQuests"), QuestCadence.Weekly, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportQuestsAsync(JsonElement quests, QuestCadence cadence, CancellationToken cancellationToken)
    {
        foreach (var questElement in quests.EnumerateArray())
        {
            var slug = Text(questElement, "slug");
            if (!await db.Quests.AnyAsync(x => x.Slug == slug, cancellationToken))
            {
                db.Quests.Add(new Quest
                {
                    Id = Guid.NewGuid(),
                    Slug = slug,
                    Title = Text(questElement, "title"),
                    Cadence = cadence,
                    RewardXp = Int(questElement, "rewardXp", 0),
                    RewardGems = Int(questElement, "rewardGems", 0)
                });
            }
        }
    }

    private async Task ImportProjectsAsync(JsonElement root, CancellationToken cancellationToken)
    {
        foreach (var projectElement in root.GetProperty("projects").EnumerateArray())
        {
            var slug = Text(projectElement, "slug");
            if (await db.Projects.AnyAsync(x => x.Slug == slug, cancellationToken))
            {
                continue;
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                Level = Text(projectElement, "level", "beginner"),
                Title = Text(projectElement, "title"),
                Duration = Text(projectElement, "duration", ""),
                XpReward = Int(projectElement, "xpReward", 120),
                DeliverablesJson = JsonSerializer.Serialize(StringArray(projectElement, "deliverables"))
            };

            foreach (var rubric in projectElement.GetProperty("rubric").EnumerateArray())
            {
                project.RubricCriteria.Add(new ProjectRubricCriterion
                {
                    Id = Guid.NewGuid(),
                    Criterion = Text(rubric, "criterion"),
                    Points = Int(rubric, "points", 0)
                });
            }

            db.Projects.Add(project);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportNotificationsAsync(JsonElement root, CancellationToken cancellationToken)
    {
        foreach (var templateElement in root.GetProperty("notificationTemplates").EnumerateArray())
        {
            var slug = Text(templateElement, "key");
            if (!await db.NotificationTemplates.AnyAsync(x => x.Slug == slug, cancellationToken))
            {
                db.NotificationTemplates.Add(new NotificationTemplate
                {
                    Id = Guid.NewGuid(),
                    Slug = slug,
                    Channel = Text(templateElement, "channel", "web"),
                    Title = Text(templateElement, "title"),
                    Body = Text(templateElement, "body")
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportExperimentsAsync(JsonElement root, CancellationToken cancellationToken)
    {
        foreach (var experimentElement in root.GetProperty("experimentation").GetProperty("candidateABTests").EnumerateArray())
        {
            var key = Text(experimentElement, "key");
            if (await db.Experiments.AnyAsync(x => x.Key == key, cancellationToken))
            {
                continue;
            }

            var experiment = new Experiment
            {
                Id = Guid.NewGuid(),
                Key = key,
                Name = key.Replace('_', ' '),
                Hypothesis = Text(experimentElement, "hypothesis"),
                PrimaryMetric = Text(experimentElement, "primaryMetric"),
                GuardrailMetric = Text(experimentElement, "guardrail"),
                IsActive = true
            };
            foreach (var variant in StringArray(experimentElement, "variants"))
            {
                experiment.Variants.Add(new ExperimentVariant { Id = Guid.NewGuid(), Key = variant, Weight = 1 });
            }

            db.Experiments.Add(experiment);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ImportSubscriptionPlansAsync(CancellationToken cancellationToken)
    {
        if (await db.SubscriptionPlans.AnyAsync(cancellationToken))
        {
            return;
        }

        db.SubscriptionPlans.AddRange(
            new SubscriptionPlan { Id = Guid.NewGuid(), Slug = "free", Name = "Free", MonthlyAiMessageLimit = 10 },
            new SubscriptionPlan { Id = Guid.NewGuid(), Slug = "plus", Name = "Plus", MonthlyAiMessageLimit = 200 },
            new SubscriptionPlan { Id = Guid.NewGuid(), Slug = "pro", Name = "Pro", MonthlyAiMessageLimit = 1000 });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string Text(JsonElement element, string property, string fallback = "")
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ValueKind == JsonValueKind.String ? value.GetString() ?? fallback : value.ToString()
            : fallback;
    }

    private static int Int(JsonElement element, string property, int fallback)
    {
        return element.TryGetProperty(property, out var value) && value.TryGetInt32(out var parsed) ? parsed : fallback;
    }

    private static bool Bool(JsonElement element, string property, bool fallback)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : fallback;
    }

    private static string[] StringArray(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return [];
        }

        return value.ValueKind switch
        {
            JsonValueKind.Array => value.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray(),
            JsonValueKind.String => value.GetString()?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [],
            _ => []
        };
    }

    private static ExerciseType MapExerciseType(string type)
    {
        return type switch
        {
            "multiple_choice" => ExerciseType.MultipleChoice,
            "code_fill_blank" => ExerciseType.CodeFill,
            "debug_task" => ExerciseType.Debugging,
            "prompt_rewrite" => ExerciseType.PromptRewrite,
            _ => ExerciseType.ShortAnswer
        };
    }

    private static int LessonOrder(string lessonSlug)
    {
        var suffix = lessonSlug.Split('-').LastOrDefault();
        return int.TryParse(suffix, out var order) ? order : lessonSlug.Contains("checkpoint", StringComparison.OrdinalIgnoreCase) ? 99 : 0;
    }
}
