using System.Text;
using System.Threading.RateLimiting;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.AiMentor;
using AiEducation.Api.Features.Auth;
using AiEducation.Api.Features.Experimentation;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Features.Learning;
using AiEducation.Api.Features.Leagues;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var provider = builder.Configuration.GetValue("DatabaseProvider", "SqlServer");
        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = builder.Configuration.GetConnectionString("SqliteConnection")
                ?? "Data Source=ai_education_platform_v2.db";
            options.UseSqlite(connectionString);
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=AiEducationPlatformV2;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            options.UseSqlServer(connectionString);
        }
    });
}

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (!builder.Environment.IsDevelopment()
    && !builder.Environment.IsEnvironment("Testing")
    && jwtOptions.SigningKey == new JwtOptions().SigningKey)
{
    throw new InvalidOperationException("Production JWT signing key must be provided via configuration.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy => policy
        .WithOrigins("http://localhost:5173", "https://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("ai", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<XpService>();
builder.Services.AddScoped<StreakService>();
builder.Services.AddScoped<QuestProgressService>();
builder.Services.AddScoped<LessonCompletionService>();
builder.Services.AddScoped<LeagueService>();
builder.Services.AddScoped<ExperimentAssignmentService>();
builder.Services.AddScoped<SeedDataImporter>();
builder.Services.AddScoped<IAiMentorClient, MockAiMentorClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlServer())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
        if (db.Database.IsSqlite())
        {
            await EnsureSqliteFallbackSchemaAsync(db);
        }
    }
    var importer = scope.ServiceProvider.GetRequiredService<SeedDataImporter>();
    await importer.ImportAsync();
}

app.UseHttpsRedirection();
app.UseCors("web");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureSqliteFallbackSchemaAsync(AppDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "NotificationPreferences" (
            "UserId" TEXT NOT NULL CONSTRAINT "PK_NotificationPreferences" PRIMARY KEY,
            "MorningReminderEnabled" INTEGER NOT NULL,
            "StreakReminderEnabled" INTEGER NOT NULL,
            "ProjectReminderEnabled" INTEGER NOT NULL,
            "EmailEnabled" INTEGER NOT NULL,
            "UpdatedAtUtc" TEXT NOT NULL,
            CONSTRAINT "FK_NotificationPreferences_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "QuizQuestions" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_QuizQuestions" PRIMARY KEY,
            "LessonId" TEXT NOT NULL,
            "Prompt" TEXT NOT NULL,
            "Explanation" TEXT NOT NULL,
            "SortOrder" INTEGER NOT NULL,
            CONSTRAINT "FK_QuizQuestions_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
        );
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "QuizOptions" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_QuizOptions" PRIMARY KEY,
            "QuizQuestionId" TEXT NOT NULL,
            "Text" TEXT NOT NULL,
            "IsCorrect" INTEGER NOT NULL,
            "SortOrder" INTEGER NOT NULL,
            CONSTRAINT "FK_QuizOptions_QuizQuestions_QuizQuestionId" FOREIGN KEY ("QuizQuestionId") REFERENCES "QuizQuestions" ("Id") ON DELETE CASCADE
        );
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "UserExerciseSubmissions" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_UserExerciseSubmissions" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "LessonId" TEXT NOT NULL,
            "ExerciseId" TEXT NOT NULL,
            "Answer" TEXT NOT NULL,
            "Feedback" TEXT NOT NULL,
            "PassedQualityGate" INTEGER NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            CONSTRAINT "FK_UserExerciseSubmissions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_UserExerciseSubmissions_Exercises_ExerciseId" FOREIGN KEY ("ExerciseId") REFERENCES "Exercises" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_UserExerciseSubmissions_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE RESTRICT
        );
        """);

    await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS "IX_QuizOptions_QuizQuestionId" ON "QuizOptions" ("QuizQuestionId");""");
    await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS "IX_QuizQuestions_LessonId" ON "QuizQuestions" ("LessonId");""");
    await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS "IX_UserExerciseSubmissions_ExerciseId" ON "UserExerciseSubmissions" ("ExerciseId");""");
    await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS "IX_UserExerciseSubmissions_LessonId" ON "UserExerciseSubmissions" ("LessonId");""");
    await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS "IX_UserExerciseSubmissions_UserId_ExerciseId_CreatedAtUtc" ON "UserExerciseSubmissions" ("UserId", "ExerciseId", "CreatedAtUtc");""");
    await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS "IX_XpTransactions_UserId_EventType_ReferenceType_ReferenceId" ON "XpTransactions" ("UserId", "EventType", "ReferenceType", "ReferenceId");""");

    try
    {
        await db.Database.ExecuteSqlRawAsync("""ALTER TABLE "LeagueSeasons" ADD "GroupNumber" INTEGER NOT NULL DEFAULT 1;""");
    }
    catch (Microsoft.Data.Sqlite.SqliteException exception) when (exception.SqliteErrorCode == 1 && exception.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
    {
    }
}

public partial class Program;
