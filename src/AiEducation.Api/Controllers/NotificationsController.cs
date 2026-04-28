using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(AppDbContext db) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var logs = await db.NotificationLogs
            .Include(x => x.NotificationTemplate)
            .Where(x => x.UserId == CurrentUserId())
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.CreatedAtUtc,
                Template = x.NotificationTemplate == null ? null : new { x.NotificationTemplate.Title, x.NotificationTemplate.Body }
            })
            .ToListAsync(cancellationToken);
        logs = logs.OrderByDescending(x => x.CreatedAtUtc).ToList();

        if (logs.Count == 0)
        {
            var template = await db.NotificationTemplates.FirstOrDefaultAsync(cancellationToken);
            if (template is not null)
            {
                var log = new NotificationLog
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUserId(),
                    NotificationTemplateId = template.Id,
                    Status = "sent",
                    CreatedAtUtc = DateTimeOffset.UtcNow
                };
                db.NotificationLogs.Add(log);
                await db.SaveChangesAsync(cancellationToken);
                logs = await db.NotificationLogs
                    .Include(x => x.NotificationTemplate)
                    .Where(x => x.UserId == CurrentUserId())
                    .Select(x => new
                    {
                        x.Id,
                        x.Status,
                        x.CreatedAtUtc,
                        Template = x.NotificationTemplate == null ? null : new { x.NotificationTemplate.Title, x.NotificationTemplate.Body }
                    })
                    .ToListAsync(cancellationToken);
                logs = logs.OrderByDescending(x => x.CreatedAtUtc).ToList();
            }
        }

        return Ok(logs);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id, CancellationToken cancellationToken)
    {
        var log = await db.NotificationLogs.SingleOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId(), cancellationToken);
        if (log is null)
        {
            return NotFound();
        }

        log.Status = "opened";
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
