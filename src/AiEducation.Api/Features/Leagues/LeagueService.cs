using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Leagues;

public sealed class LeagueService(AppDbContext db)
{
    public static IReadOnlyList<LeagueResult> CalculateSeasonResults(
        IReadOnlyCollection<LeagueParticipant> participants,
        DateTimeOffset seasonEndsAtUtc)
    {
        return participants
            .OrderByDescending(x => x.WeeklyXp)
            .ThenBy(x => x.JoinedAtUtc)
            .Select((participant, index) =>
            {
                var rank = index + 1;
                var isNewUserProtected = seasonEndsAtUtc - participant.JoinedAtUtc < TimeSpan.FromDays(14);
                var movement = rank <= 10
                    ? LeagueMovement.Promote
                    : rank >= Math.Max(1, participants.Count - 4) && !isNewUserProtected
                        ? LeagueMovement.Demote
                        : LeagueMovement.Stay;

                return new LeagueResult(participant.UserId, rank, participant.LeagueTier, movement);
            })
            .ToList();
    }

    public async Task<LeagueSeason> GetOrCreateCurrentSeasonAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var seasons = (await db.LeagueSeasons
            .Include(x => x.Participants)
            .ToListAsync(cancellationToken))
            .Where(x => x.StartsAtUtc <= now && x.EndsAtUtc > now)
            .OrderBy(x => x.GroupNumber)
            .ToList();

        var season = seasons.FirstOrDefault(x => x.Participants.Any(p => p.UserId == userId))
            ?? seasons.FirstOrDefault(x => x.Participants.Count < 30);
        if (season is null)
        {
            var start = StartOfWeekUtc(now);
            season = new LeagueSeason
            {
                Id = Guid.NewGuid(),
                Name = $"Bronze {start:yyyy-MM-dd} G{seasons.Count + 1}",
                StartsAtUtc = start,
                EndsAtUtc = start.AddDays(7),
                Tier = LeagueTier.Bronze,
                GroupNumber = seasons.Select(x => x.GroupNumber).DefaultIfEmpty(0).Max() + 1
            };
            db.LeagueSeasons.Add(season);
        }

        var weeklyXp = await WeeklyXpAsync(userId, season.StartsAtUtc, season.EndsAtUtc, cancellationToken);
        if (season.Participants.All(x => x.UserId != userId))
        {
            var user = await db.Users.FindAsync([userId], cancellationToken);
            var participant = new LeagueParticipant
            {
                Id = Guid.NewGuid(),
                LeagueSeasonId = season.Id,
                UserId = userId,
                LeagueTier = LeagueTier.Bronze,
                DisplayName = user?.DisplayName ?? "Learner",
                JoinedAtUtc = user?.CreatedAtUtc ?? now,
                WeeklyXp = weeklyXp
            };
            db.LeagueParticipants.Add(participant);
        }
        else
        {
            season.Participants.Single(x => x.UserId == userId).WeeklyXp = weeklyXp;
        }

        var ranked = season.Participants.OrderByDescending(x => x.WeeklyXp).ThenBy(x => x.JoinedAtUtc).ToList();
        for (var index = 0; index < ranked.Count; index++)
        {
            ranked[index].Rank = index + 1;
        }

        await db.SaveChangesAsync(cancellationToken);
        return season;
    }

    public async Task RefreshWeeklyXpAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var season = (await db.LeagueSeasons
            .Include(x => x.Participants)
            .ToListAsync(cancellationToken))
            .FirstOrDefault(x => x.StartsAtUtc <= now && x.EndsAtUtc > now && x.Participants.Any(p => p.UserId == userId));
        if (season is null)
        {
            return;
        }

        var participant = season.Participants.Single(x => x.UserId == userId);
        participant.WeeklyXp = await WeeklyXpAsync(userId, season.StartsAtUtc, season.EndsAtUtc, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RefreshCurrentSeasonAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var seasons = (await db.LeagueSeasons
            .Include(x => x.Participants)
            .ToListAsync(cancellationToken))
            .Where(x => x.StartsAtUtc <= now && x.EndsAtUtc > now)
            .ToList();

        foreach (var participant in seasons.SelectMany(x => x.Participants))
        {
            var season = seasons.Single(x => x.Id == participant.LeagueSeasonId);
            participant.WeeklyXp = await WeeklyXpAsync(participant.UserId, season.StartsAtUtc, season.EndsAtUtc, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CloseSeasonAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        var season = await db.LeagueSeasons
            .Include(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == seasonId, cancellationToken);
        if (season is null || season.ClosedAtUtc is not null)
        {
            return;
        }

        foreach (var participant in season.Participants)
        {
            participant.WeeklyXp = await WeeklyXpAsync(participant.UserId, season.StartsAtUtc, season.EndsAtUtc, cancellationToken);
        }

        var results = CalculateSeasonResults(season.Participants, season.EndsAtUtc);
        foreach (var result in results)
        {
            var participant = season.Participants.Single(x => x.UserId == result.UserId);
            participant.Rank = result.Rank;
            switch (result.Movement)
            {
                case LeagueMovement.Promote:
                    participant.LeagueTier = Promote(participant.LeagueTier);
                    AddLeagueEvent(AnalyticsEvents.LeagueUserPromoted, participant, season);
                    break;
                case LeagueMovement.Demote:
                    participant.LeagueTier = Demote(participant.LeagueTier);
                    AddLeagueEvent(AnalyticsEvents.LeagueUserDemoted, participant, season);
                    break;
                case LeagueMovement.Stay when season.EndsAtUtc - participant.JoinedAtUtc < TimeSpan.FromDays(14) && result.Rank >= Math.Max(1, season.Participants.Count - 4):
                    AddLeagueEvent(AnalyticsEvents.LeagueUserProtected, participant, season);
                    break;
            }
        }

        season.ClosedAtUtc = DateTimeOffset.UtcNow;
        db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            Id = Guid.NewGuid(),
            EventName = AnalyticsEvents.LeagueSeasonClosed,
            PropertiesJson = $$"""{"seasonId":"{{season.Id}}","participantCount":{{season.Participants.Count}}}""",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        await EnsureNextSeasonAsync(cancellationToken);
    }

    public async Task EnsureNextSeasonAsync(CancellationToken cancellationToken = default)
    {
        var latest = (await db.LeagueSeasons.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.EndsAtUtc)
            .FirstOrDefault();
        if (latest is null)
        {
            var start = StartOfWeekUtc(DateTimeOffset.UtcNow);
            db.LeagueSeasons.Add(new LeagueSeason
            {
                Id = Guid.NewGuid(),
                Name = $"Bronze {start:yyyy-MM-dd} G1",
                StartsAtUtc = start,
                EndsAtUtc = start.AddDays(7),
                Tier = LeagueTier.Bronze,
                GroupNumber = 1
            });
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var nextStart = latest.EndsAtUtc;
        var exists = await db.LeagueSeasons.AnyAsync(x => x.StartsAtUtc == nextStart && x.GroupNumber == latest.GroupNumber, cancellationToken);
        if (!exists)
        {
            db.LeagueSeasons.Add(new LeagueSeason
            {
                Id = Guid.NewGuid(),
                Name = $"{latest.Tier} {nextStart:yyyy-MM-dd} G{latest.GroupNumber}",
                StartsAtUtc = nextStart,
                EndsAtUtc = nextStart.AddDays(7),
                Tier = latest.Tier,
                GroupNumber = latest.GroupNumber
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RolloverIfNeededAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var seasons = (await db.LeagueSeasons.ToListAsync(cancellationToken))
            .Where(x => x.EndsAtUtc <= now && x.ClosedAtUtc == null)
            .Select(x => x.Id)
            .ToList();

        foreach (var seasonId in seasons)
        {
            await CloseSeasonAsync(seasonId, cancellationToken);
        }

        await EnsureNextSeasonAsync(cancellationToken);
    }

    private static DateTimeOffset StartOfWeekUtc(DateTimeOffset now)
    {
        var date = now.UtcDateTime.Date;
        var delta = ((int)date.DayOfWeek + 6) % 7;
        return new DateTimeOffset(date.AddDays(-delta), TimeSpan.Zero);
    }

    private static LeagueTier Promote(LeagueTier tier)
    {
        return tier == LeagueTier.Diamond ? tier : (LeagueTier)((int)tier + 1);
    }

    private static LeagueTier Demote(LeagueTier tier)
    {
        return tier == LeagueTier.Bronze ? tier : (LeagueTier)((int)tier - 1);
    }

    private void AddLeagueEvent(string eventName, LeagueParticipant participant, LeagueSeason season)
    {
        db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            Id = Guid.NewGuid(),
            UserId = participant.UserId,
            EventName = eventName,
            PropertiesJson = $$"""{"seasonId":"{{season.Id}}","rank":{{participant.Rank}},"tier":"{{participant.LeagueTier}}"}""",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
    }

    private async Task<int> WeeklyXpAsync(Guid userId, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, CancellationToken cancellationToken)
    {
        var candidates = await db.XpTransactions
            .Where(x => x.UserId == userId
                && x.PassedQualityGate)
            .ToListAsync(cancellationToken);
        return candidates
            .Where(x => x.CreatedAtUtc >= startsAtUtc && x.CreatedAtUtc < endsAtUtc)
            .Sum(x => x.Amount);
    }
}
