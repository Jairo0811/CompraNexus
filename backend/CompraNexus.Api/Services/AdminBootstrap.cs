using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Services;

public static class AdminBootstrap
{
    // Solo se ejecuta en Development, con contraseña elegida por el operador.
    // Nunca sobreescribe usuarios existentes ni registra contraseñas en logs.
    public static async Task EnsureInitialAdminAsync(
        CompraNexusDbContext db, IConfiguration config, ILogger logger, CancellationToken ct = default)
    {
        if (await db.AppUsers.AnyAsync(ct)) return;
        var email = config["Auth:BootstrapEmail"]?.Trim();
        var password = config["Auth:BootstrapPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("No hay usuarios. Configure COMPRANEXUS_ADMIN_EMAIL y COMPRANEXUS_ADMIN_PASSWORD para crear el primer administrador.");
            return;
        }
        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)
            || email.Length > 180 || password.Length < 14 || password.Length > 128)
            throw new InvalidOperationException("Correo de administrador inválido o contraseña inicial fuera del rango de 14 a 128 caracteres.");

        var user = new AppUser
        {
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Role = ApplicationRoles.Administrator
        };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
        db.AppUsers.Add(user);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Administrador inicial creado. Retire COMPRANEXUS_ADMIN_PASSWORD del archivo .env.");
    }
}
