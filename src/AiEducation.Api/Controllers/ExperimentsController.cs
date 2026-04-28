using AiEducation.Api.Data;
using AiEducation.Api.Features.Experimentation;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Route("api/v1")]
public sealed class ExperimentsController(AppDbContext db, ExperimentAssignmentService assignmentService) : ApiControllerBase
{
    [HttpGet("experiments/assignments")]
    [Authorize]
    public async Task<IActionResult> Assignments(CancellationToken cancellationToken)
    {
        var experiments = await db.Experiments.Where(x => x.IsActive).Select(x => x.Key).ToListAsync(cancellationToken);
        var assignments = new List<object>();
        foreach (var key in experiments)
        {
            var assignment = await assignmentService.AssignAsync(CurrentUserId(), key, cancellationToken);
            assignments.Add(new { assignment.ExperimentKey, assignment.VariantKey });
        }

        return Ok(assignments);
    }

    [HttpPost("admin/experiments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(AdminExperimentRequest request, CancellationToken cancellationToken)
    {
        var experiment = new Experiment
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Name = request.Name,
            Hypothesis = request.Hypothesis,
            PrimaryMetric = request.PrimaryMetric,
            GuardrailMetric = request.GuardrailMetric,
            IsActive = request.IsActive,
            Variants = request.Variants.Select(x => new ExperimentVariant { Id = Guid.NewGuid(), Key = x, Weight = 1 }).ToList()
        };
        db.Experiments.Add(experiment);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/experiments/{experiment.Id}", new { experiment.Id, experiment.Key });
    }

    [HttpPatch("admin/experiments/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateExperimentRequest request, CancellationToken cancellationToken)
    {
        var experiment = await db.Experiments.FindAsync([id], cancellationToken);
        if (experiment is null)
        {
            return NotFound();
        }

        experiment.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { experiment.Id, experiment.IsActive });
    }
}

public sealed record AdminExperimentRequest(string Key, string Name, string Hypothesis, string PrimaryMetric, string GuardrailMetric, bool IsActive, string[] Variants);
public sealed record UpdateExperimentRequest(bool IsActive);
