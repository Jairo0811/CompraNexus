using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Controllers;

public sealed record LoginInput([Required, EmailAddress] string Email, [Required] string Password);
public sealed record SessionView(int Id, string Email, string Role);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(CompraNexusDbContext db, IPasswordHasher<AppUser> hasher) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult Csrf([FromServices] IAntiforgery antiforgery)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginInput input, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var key = input.Email.Trim().ToUpperInvariant();
        var user = await db.AppUsers.SingleOrDefaultAsync(x => x.NormalizedEmail == key, ct);
        var now = DateTimeOffset.UtcNow;
        if (user is null || !user.IsActive || user.LockoutEndUtc > now)
            return Unauthorized(new { message = "Credenciales incorrectas o acceso temporalmente bloqueado." });

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedAttempts++;
            if (user.FailedAttempts >= 5)
            {
                user.LockoutEndUtc = now.AddMinutes(15);
                user.FailedAttempts = 0;
            }
            await db.SaveChangesAsync(ct);
            return Unauthorized(new { message = "Credenciales incorrectas o acceso temporalmente bloqueado." });
        }

        user.FailedAttempts = 0;
        user.LockoutEndUtc = null;
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = hasher.HashPassword(user, input.Password);
        await db.SaveChangesAsync(ct);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("SecurityStamp", user.SecurityStamp)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
        return Ok(new SessionView(user.Id, user.Email, user.Role));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        if (User.Identity?.IsAuthenticated != true) return Unauthorized();
        var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idString, out var id)) return Unauthorized();
        var user = await db.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        return user is null ? Unauthorized() : Ok(new SessionView(user.Id, user.Email, user.Role));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
