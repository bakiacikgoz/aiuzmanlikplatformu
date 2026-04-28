using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Route("api/v1")]
public sealed class AnalyticsController(AppDbContext db, AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpPost("events/track")]
    public async Task<IActionResult> Track(TrackEventRequest request, CancellationToken cancellationToken)
    {
        await analyticsService.TrackAsync(
            User.Identity?.IsAuthenticated == true ? CurrentUserId() : null,
            request.EventName,
            request.Properties ?? new Dictionary<string, object>(),
            DateTimeOffset.UtcNow,
            cancellationToken);
        return Accepted(new { tracked = true });
    }

    [HttpGet("admin/analytics/funnel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Funnel(CancellationToken cancellationToken)
    {
        var counts = await db.AnalyticsEvents.GroupBy(x => x.EventName).Select(x => new { EventName = x.Key, Count = x.Count() }).ToListAsync(cancellationToken);
        return Ok(counts);
    }

    [HttpGet("admin/analytics/retention")]
    [Authorize(Roles = "Admin")]
    public IActionResult Retention()
    {
        return Ok(new { d1 = 0, d7 = 0, d30 = 0, note = "MVP placeholder based on tracked events." });
    }
}

public sealed record TrackEventRequest(string EventName, Dictionary<string, object>? Properties);
