using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Services;

/// <summary>
/// Recuperación local, excepcional y no accesible mediante HTTP.
/// Reutiliza el PasswordHasher del login y revoca las sesiones por SecurityStamp.
/// </summary>
public static class AdminPasswordRecovery
{
    public const int MinimumPasswordLength = 14;
    public const int MaximumPasswordLength = 128;

    public static async Task ResetAsync(
        CompraNexusDbContext db, IPasswordHasher<AppUser> hasher,
        string email, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Debe indicar el correo del administrador.", nameof(email));
        if (newPassword is null
            || newPassword.Length < MinimumPasswordLength
            || newPassword.Length > MaximumPasswordLength)
            throw new ArgumentException("La nueva contraseña debe tener entre 14 y 128 caracteres.", nameof(newPassword));

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await db.AppUsers.SingleOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || user.Role != ApplicationRoles.Administrator || !user.IsActive)
            throw new InvalidOperationException("No se encontró un administrador activo con ese correo.");

        user.PasswordHash = hasher.HashPassword(user, newPassword);
        user.FailedAttempts = 0;
        user.LockoutEndUtc = null;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        await db.SaveChangesAsync(cancellationToken);
    }
}
