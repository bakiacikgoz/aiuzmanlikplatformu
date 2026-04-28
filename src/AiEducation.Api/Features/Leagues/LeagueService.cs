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
        var season = await db.LeagueSeasons
            .Include(x => x.Participants)
            .Where(x => x.StartsAtUtc <= now && x.EndsAtUtc > now)
            .OrderByDescending(x => x.StartsAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (season is null)
        {
            var start = StartOfWeekUtc(now);
            season = new LeagueSeason
            {
                Id = Guid.NewGuid(),
                Name = $"Bronze {start:yyyy-MM-dd}",
                StartsAtUtc = start,
                EndsAtUtc = start.AddDays(7),
                Tier = LeagueTier.Bronze
            };
            db.LeagueSeasons.Add(season);
        }

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
                WeeklyXp = await db.XpTransactions
                    .Where(x => x.UserId == userId && x.PassedQualityGate && x.CreatedAtUtc >= season.StartsAtUtc)
                    .SumAsync(x => x.Amount, cancellationToken)
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return season;
    }

    private static DateTimeOffset StartOfWeekUtc(DateTimeOffset now)
    {
        var date = now.UtcDateTime.Date;
        var delta = ((int)date.DayOfWeek + 6) % 7;
        return new DateTimeOffset(date.AddDays(-delta), TimeSpan.Zero);
    }
}
