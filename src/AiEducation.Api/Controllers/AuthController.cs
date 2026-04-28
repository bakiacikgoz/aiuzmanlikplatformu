using System.ComponentModel.DataAnnotations;
using AiEducation.Api.Data;
using AiEducation.Api.Features.Analytics;
using AiEducation.Api.Features.Auth;
using AiEducation.Api.Infrastructure;
using AiEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AiEducation.Api.Controllers;

[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    TokenService tokenService,
    AppDbContext db,
    AnalyticsService analyticsService) : ApiControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!new EmailAddressAttribute().IsValid(request.Email)
            || string.IsNullOrWhiteSpace(request.DisplayName)
            || string.IsNullOrWhiteSpace(request.Password)
            || request.Password.Length < 8)
        {
            return BadRequest(new { message = "Geçerli e-posta, en az 8 karakter şifre ve ad zorunludur." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            TimeZoneId = request.TimeZoneId,
            DailyXpGoal = 20,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationProblem(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await EnsureRoleAsync("Learner");
        await userManager.AddToRoleAsync(user, "Learner");
        await EnsureFreeSubscriptionAsync(user.Id, cancellationToken);
        db.UserStreaks.Add(new UserStreak { UserId = user.Id });
        await db.SaveChangesAsync(cancellationToken);
        await analyticsService.TrackAsync(user.Id, AnalyticsEvents.UserSignedUp, new { user.Email }, user.CreatedAtUtc, cancellationToken);

        var auth = await tokenService.IssueAsync(user, cancellationToken);
        SetRefreshCookie(auth.RefreshToken, auth.ExpiresAtUtc.AddDays(14));
        return new AuthResponse(auth.AccessToken, auth.ExpiresAtUtc, user.Id, user.DisplayName, user.Email ?? "", ["Learner"]);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var auth = await tokenService.IssueAsync(user, cancellationToken);
        SetRefreshCookie(auth.RefreshToken, auth.ExpiresAtUtc.AddDays(14));
        var roles = await userManager.GetRolesAsync(user);
        await analyticsService.TrackAsync(user.Id, AnalyticsEvents.UserLoggedIn, new { user.Email }, DateTimeOffset.UtcNow, cancellationToken);
        return new AuthResponse(auth.AccessToken, auth.ExpiresAtUtc, user.Id, user.DisplayName, user.Email ?? "", roles.ToArray());
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = request.RefreshToken ?? Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var tokenHash = TokenService.HashRefreshToken(refreshToken);
        var stored = await db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(
            x => x.TokenHash == tokenHash && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTimeOffset.UtcNow,
            cancellationToken);
        if (stored?.User is null)
        {
            return Unauthorized();
        }

        stored.RevokedAtUtc = DateTimeOffset.UtcNow;
        var auth = await tokenService.IssueAsync(stored.User, cancellationToken);
        SetRefreshCookie(auth.RefreshToken, auth.ExpiresAtUtc.AddDays(14));
        var roles = await userManager.GetRolesAsync(stored.User);
        return new AuthResponse(auth.AccessToken, auth.ExpiresAtUtc, stored.User.Id, stored.User.DisplayName, stored.User.Email ?? "", roles.ToArray());
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        await db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAtUtc, DateTimeOffset.UtcNow), cancellationToken);
        Response.Cookies.Delete("refresh_token");
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([CurrentUserId()], cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserResponse(user.Id, user.DisplayName, user.Email ?? "", user.DailyXpGoal, user.SelectedLearningPathSlug, user.TimeZoneId, roles.ToArray());
    }

    private async Task EnsureRoleAsync(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    private async Task EnsureFreeSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var freePlan = await db.SubscriptionPlans.SingleOrDefaultAsync(x => x.Slug == "free", cancellationToken);
        if (freePlan is null)
        {
            freePlan = new SubscriptionPlan { Id = Guid.NewGuid(), Slug = "free", Name = "Free", MonthlyAiMessageLimit = 10 };
            db.SubscriptionPlans.Add(freePlan);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.UserSubscriptions.AnyAsync(x => x.UserId == userId && x.Status == "active", cancellationToken))
        {
            db.UserSubscriptions.Add(new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanId = freePlan.Id,
                Status = "active",
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }
    }

    private void SetRefreshCookie(string refreshToken, DateTimeOffset expires)
    {
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expires
        });
    }
}

public sealed record RegisterRequest(string Email, string Password, string DisplayName, string TimeZoneId);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string? RefreshToken);
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, Guid UserId, string DisplayName, string Email, string[] Roles);
public sealed record UserResponse(Guid Id, string DisplayName, string Email, int DailyXpGoal, string? SelectedLearningPathSlug, string TimeZoneId, string[] Roles);
