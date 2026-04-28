using AiEducation.Api.Data;
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
        var seasons = await db.LeagueSeasons
            .Include(x => x.Participants)
            .Where(x => x.StartsAtUtc <= now && x.EndsAtUtc > now)
            .OrderBy(x => x.GroupNumber)
            .ToListAsync(cancellationToken);

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
            season.Participants.Add(new LeagueParticipant
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueTier = LeagueTier.Bronze,
                DisplayName = user?.DisplayName ?? "Learner",
                JoinedAtUtc = user?.CreatedAtUtc ?? now,
                WeeklyXp = weeklyXp
            });
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
        var season = await db.LeagueSeasons
            .Include(x => x.Participants)
            .Where(x => x.StartsAtUtc <= now && x.EndsAtUtc > now && x.Participants.Any(p => p.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
        if (season is null)
        {
            return;
        }

        var participant = season.Participants.Single(x => x.UserId == userId);
        participant.WeeklyXp = await WeeklyXpAsync(userId, season.StartsAtUtc, season.EndsAtUtc, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset StartOfWeekUtc(DateTimeOffset now)
    {
        var date = now.UtcDateTime.Date;
        var delta = ((int)date.DayOfWeek + 6) % 7;
        return new DateTimeOffset(date.AddDays(-delta), TimeSpan.Zero);
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
