using System.Text.Json;
using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Route("api/v1")]
public sealed class AnalyticsController(AppDbContext db) : ApiControllerBase
{
    [HttpPost("events/track")]
    public async Task<IActionResult> Track(TrackEventRequest request, CancellationToken cancellationToken)
    {
        db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            Id = Guid.NewGuid(),
            UserId = User.Identity?.IsAuthenticated == true ? CurrentUserId() : null,
            EventName = request.EventName,
            PropertiesJson = JsonSerializer.Serialize(request.Properties ?? new Dictionary<string, object>()),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return Accepted(new { tracked = true });
    }

    [HttpGet("admin/analytics/funnel")]
    [Authorize]
    public async Task<IActionResult> Funnel(CancellationToken cancellationToken)
    {
        var counts = await db.AnalyticsEvents.GroupBy(x => x.EventName).Select(x => new { EventName = x.Key, Count = x.Count() }).ToListAsync(cancellationToken);
        return Ok(counts);
    }

    [HttpGet("admin/analytics/retention")]
    [Authorize]
    public IActionResult Retention()
    {
        return Ok(new { d1 = 0, d7 = 0, d30 = 0, note = "MVP placeholder based on tracked events." });
    }
}

public sealed record TrackEventRequest(string EventName, Dictionary<string, object>? Properties);
