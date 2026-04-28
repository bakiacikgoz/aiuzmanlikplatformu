using System.Text.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.Gamification;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Route("api/v1")]
public sealed class ProjectsController(AppDbContext db, XpService xpService, AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpGet("projects")]
    public async Task<IActionResult> Projects(CancellationToken cancellationToken)
    {
        var projects = await db.Projects
            .Include(x => x.RubricCriteria)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Title)
            .Select(x => new
            {
                x.Slug,
                x.Level,
                x.Title,
                x.Duration,
                x.XpReward,
                Deliverables = JsonSerializer.Deserialize<string[]>(x.DeliverablesJson, (JsonSerializerOptions?)null) ?? Array.Empty<string>(),
                Rubric = x.RubricCriteria.Select(r => new { r.Criterion, r.Points })
            })
            .ToListAsync(cancellationToken);
        return Ok(projects);
    }

    [HttpGet("projects/{slug}")]
    public async Task<IActionResult> Project(string slug, CancellationToken cancellationToken)
    {
        var project = await db.Projects.Include(x => x.RubricCriteria).SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            project.Slug,
            project.Level,
            project.Title,
            project.Duration,
            project.XpReward,
            Deliverables = JsonSerializer.Deserialize<string[]>(project.DeliverablesJson, (JsonSerializerOptions?)null) ?? [],
            Rubric = project.RubricCriteria.Select(x => new { x.Criterion, x.Points })
        });
    }

    [HttpPost("projects/{slug}/submissions")]
    [Authorize]
    public async Task<IActionResult> Submit(string slug, ProjectSubmissionRequest request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        if (!IsValidHttpUrl(request.RepositoryUrl) && !IsValidHttpUrl(request.DemoUrl))
        {
            return BadRequest(new { message = "Geçerli bir GitHub veya demo URL'si gir." });
        }

        var now = DateTimeOffset.UtcNow;
        var submission = new ProjectSubmission
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId(),
            ProjectId = project.Id,
            RepositoryUrl = request.RepositoryUrl,
            DemoUrl = request.DemoUrl ?? "",
            Notes = request.Notes ?? "",
            CreatedAtUtc = now
        };
        submission.PortfolioEvidence.Add(new PortfolioEvidence
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId(),
            Title = project.Title,
            EvidenceUrl = string.IsNullOrWhiteSpace(request.RepositoryUrl) ? request.DemoUrl ?? "" : request.RepositoryUrl,
            CreatedAtUtc = now
        });
        db.ProjectSubmissions.Add(submission);
        await db.SaveChangesAsync(cancellationToken);
        var alreadyAwarded = await db.XpTransactions.AnyAsync(
            x => x.UserId == CurrentUserId()
                && x.EventType == XpEvents.ProjectSubmitted
                && x.ReferenceType == "Project"
                && x.ReferenceId == project.Id,
            cancellationToken);
        var xpTransaction = await xpService.GrantXpAsync(CurrentUserId(), XpEvents.ProjectSubmitted, project.XpReward, "Project", project.Id, true, now, cancellationToken);
        await analyticsService.TrackAsync(CurrentUserId(), AnalyticsEvents.ProjectSubmitted, new
        {
            project.Slug,
            submission.Id,
            xpGranted = alreadyAwarded ? 0 : xpTransaction.Amount
        }, now, cancellationToken);

        return Ok(new { submission.Id, portfolioEvidenceCreated = true, xpGranted = alreadyAwarded ? 0 : xpTransaction.Amount });
    }

    [HttpGet("portfolio/me")]
    [Authorize]
    public async Task<IActionResult> Portfolio(CancellationToken cancellationToken)
    {
        var evidence = await db.PortfolioEvidence
            .Where(x => x.UserId == CurrentUserId())
            .Select(x => new { x.Title, x.EvidenceUrl, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(evidence.OrderByDescending(x => x.CreatedAtUtc));
    }

    private static bool IsValidHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https";
    }
}

public sealed record ProjectSubmissionRequest(string RepositoryUrl, string? DemoUrl, string? Notes);
