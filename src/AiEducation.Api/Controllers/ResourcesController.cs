using AiEducation.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[ApiController]
[Route("api/v1/resources")]
public sealed class ResourcesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Resources(CancellationToken cancellationToken)
    {
        var resources = await db.Resources
            .OrderBy(x => x.Title)
            .Select(x => new { x.Slug, x.Title, x.Url, x.Type, x.Summary })
            .ToListAsync(cancellationToken);
        return Ok(resources);
    }
}
