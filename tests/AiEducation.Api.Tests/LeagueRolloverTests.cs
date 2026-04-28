using System.Net.Http.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Features.Leagues;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Tests;

public sealed class LeagueRolloverTests
{
    [Fact]
    public async Task League_rollover_promotes_demotes_protects_new_users_and_is_idempotent()
    {
        await using var db = TestDb.Create();
        var season = new LeagueSeason
        {
            Id = Guid.NewGuid(),
            Name = "Bronze test",
            StartsAtUtc = DateTimeOffset.Parse("2026-04-20T00:00:00Z"),
            EndsAtUtc = DateTimeOffset.Parse("2026-04-27T00:00:00Z"),
            Tier = LeagueTier.Bronze,
            GroupNumber = 1
        };
        db.LeagueSeasons.Add(season);

        for (var index = 1; index <= 30; index++)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = $"league-{index}@example.com",
                Email = $"league-{index}@example.com",
                DisplayName = $"Learner {index}",
                CreatedAtUtc = index >= 26 ? season.EndsAtUtc.AddDays(-5) : season.EndsAtUtc.AddDays(-30)
            };
            db.Users.Add(user);
            db.LeagueParticipants.Add(new LeagueParticipant
            {
                Id = Guid.NewGuid(),
                LeagueSeasonId = season.Id,
                UserId = user.Id,
                DisplayName = user.DisplayName,
                LeagueTier = LeagueTier.Bronze,
                WeeklyXp = 310 - index * 10,
                Rank = index,
                JoinedAtUtc = user.CreatedAtUtc
            });
            db.XpTransactions.Add(new XpTransaction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EventType = XpEvents.MicroLessonCompleted,
                Amount = 310 - index * 10,
                ReferenceType = "Lesson",
                ReferenceId = Guid.NewGuid(),
                PassedQualityGate = true,
                CreatedAtUtc = season.StartsAtUtc.AddDays(1)
            });
        }

        await db.SaveChangesAsync();
        var service = new LeagueService(db);
        var closeMethod = typeof(LeagueService).GetMethod("CloseSeasonAsync", [typeof(Guid), typeof(CancellationToken)]);
        Assert.NotNull(closeMethod);

        await (Task)closeMethod!.Invoke(service, [season.Id, CancellationToken.None])!;
        await (Task)closeMethod.Invoke(service, [season.Id, CancellationToken.None])!;

        var closed = await db.LeagueSeasons.Include(x => x.Participants).SingleAsync(x => x.Id == season.Id);
        var closedAt = typeof(LeagueSeason).GetProperty("ClosedAtUtc")?.GetValue(closed);
        var promoted = await db.AnalyticsEvents.CountAsync(x => x.EventName == "LeagueUserPromoted");
        var demoted = await db.AnalyticsEvents.CountAsync(x => x.EventName == "LeagueUserDemoted");
        var protectedUsers = await db.AnalyticsEvents.CountAsync(x => x.EventName == "LeagueUserProtected");

        Assert.NotNull(closedAt);
        Assert.Equal(10, promoted);
        Assert.Equal(0, demoted);
        Assert.Equal(5, protectedUsers);
        Assert.Equal(1, await db.AnalyticsEvents.CountAsync(x => x.EventName == "LeagueSeasonClosed"));
        Assert.True(await db.LeagueSeasons.AnyAsync(x => x.StartsAtUtc == season.EndsAtUtc));
    }
}

public sealed class LeagueAdminEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Admin_can_trigger_rollover_endpoint()
    {
        var admin = factory.CreateClient();
        ApiFactory.Authorize(admin, await factory.RegisterAdminAndGetTokenAsync(admin, "admin-league@example.com"));

        var response = await admin.PostAsJsonAsync("/api/v1/admin/leagues/rollover", new { });

        response.EnsureSuccessStatusCode();
    }
}
