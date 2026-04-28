using System.Security.Cryptography;
using System.Text;
using AiEducation.Api.Data;
using AiEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Features.Experimentation;

public sealed class ExperimentAssignmentService(AppDbContext db)
{
    public async Task<ExperimentAssignment> AssignAsync(
        Guid userId,
        string experimentKey,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.ExperimentAssignments
            .SingleOrDefaultAsync(x => x.UserId == userId && x.ExperimentKey == experimentKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var experiment = await db.Experiments
            .Include(x => x.Variants)
            .SingleAsync(x => x.Key == experimentKey && x.IsActive, cancellationToken);

        var variant = ChooseVariant(userId, experiment);
        var assignment = new ExperimentAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExperimentId = experiment.Id,
            ExperimentVariantId = variant.Id,
            ExperimentKey = experiment.Key,
            VariantKey = variant.Key,
            AssignedAtUtc = DateTimeOffset.UtcNow
        };

        db.ExperimentAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    private static ExperimentVariant ChooseVariant(Guid userId, Experiment experiment)
    {
        var variants = experiment.Variants.OrderBy(x => x.Key).ToList();
        var totalWeight = variants.Sum(x => Math.Max(1, x.Weight));
        var bucket = StableBucket(userId, experiment.Key) % totalWeight;
        var cursor = 0;

        foreach (var variant in variants)
        {
            cursor += Math.Max(1, variant.Weight);
            if (bucket < cursor)
            {
                return variant;
            }
        }

        return variants[^1];
    }

    private static int StableBucket(Guid userId, string experimentKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{userId:N}:{experimentKey}"));
        return BitConverter.ToUInt16(bytes, 0);
    }
}
