using AiEducation.Api.Data;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Authorize]
[Route("api/v1/notification-preferences")]
public sealed class NotificationPreferencesController(AppDbContext db) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var preferences = await GetOrCreateAsync(CurrentUserId(), cancellationToken);
        return Ok(ToPayload(preferences));
    }

    [HttpPost]
    public async Task<IActionResult> Update(NotificationPreferenceRequest request, CancellationToken cancellationToken)
    {
        var preferences = await GetOrCreateAsync(CurrentUserId(), cancellationToken);
        preferences.MorningReminderEnabled = request.MorningReminderEnabled;
        preferences.StreakReminderEnabled = request.StreakReminderEnabled;
        preferences.ProjectReminderEnabled = request.ProjectReminderEnabled;
        preferences.EmailEnabled = request.EmailEnabled;
        preferences.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToPayload(preferences));
    }

    private async Task<NotificationPreference> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preferences = await db.NotificationPreferences.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (preferences is not null)
        {
            return preferences;
        }

        preferences = new NotificationPreference
        {
            UserId = userId,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        db.NotificationPreferences.Add(preferences);
        await db.SaveChangesAsync(cancellationToken);
        return preferences;
    }

    private static object ToPayload(NotificationPreference preferences)
    {
        return new
        {
            preferences.MorningReminderEnabled,
            preferences.StreakReminderEnabled,
            preferences.ProjectReminderEnabled,
            preferences.EmailEnabled,
            preferences.UpdatedAtUtc
        };
    }
}

public sealed record NotificationPreferenceRequest(
    bool MorningReminderEnabled,
    bool StreakReminderEnabled,
    bool ProjectReminderEnabled,
    bool EmailEnabled);
