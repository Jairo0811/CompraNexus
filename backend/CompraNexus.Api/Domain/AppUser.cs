using System.ComponentModel.DataAnnotations;

namespace CompraNexus.Api.Domain;

// Usuario de aplicación: independiente del catálogo académico de empleados.
public sealed class AppUser
{
    public int Id { get; set; }
    [Required, EmailAddress, StringLength(180)]
    public string Email { get; set; } = "";
    [Required, StringLength(180)]
    public string NormalizedEmail { get; set; } = "";
    [Required, StringLength(512)]
    public string PasswordHash { get; set; } = "";
    [Required, StringLength(40)]
    public string Role { get; set; } = "Administrador";
    public bool IsActive { get; set; } = true;
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }
    [Required, StringLength(64)]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
}

public static class ApplicationRoles
{
    public const string Administrator = "Administrador";
    public const string Purchases = "Compras";
    public const string Reports = "Consultas";
    public const string Accounting = "Contabilidad";
}
