using System.Text;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Services;

/// <summary>Comando interactivo "reset-admin"; nunca recibe claves por argumentos o variables de entorno.</summary>
public static class AdminRecoveryConsole
{
    public static async Task<int> RunAsync(
        IServiceProvider services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            Console.Error.WriteLine("Recuperación local disponible únicamente en Development.");
            return 2;
        }

        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Ejecute en una terminal interactiva: docker compose exec api dotnet CompraNexus.Api.dll reset-admin");
            Console.Error.WriteLine("No utilice -T, tuberías ni redirección de entrada.");
            return 2;
        }

        var email = configuration["Auth:BootstrapEmail"]?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("Falta Auth__BootstrapEmail / COMPRANEXUS_ADMIN_EMAIL.");
            return 2;
        }

        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CompraNexusDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
            var normalized = email.ToUpperInvariant();
            var user = await db.AppUsers.AsNoTracking()
                .SingleOrDefaultAsync(x => x.NormalizedEmail == normalized);
            if (user is null || !user.IsActive || user.Role != ApplicationRoles.Administrator)
            {
                Console.Error.WriteLine("El administrador configurado no existe o no está activo.");
                return 2;
            }

            Console.WriteLine("RECUPERACION LOCAL DE ADMINISTRADOR - CompraNexus");
            Console.WriteLine("Solo se cambiara la contrasena del administrador indicado.");
            Console.Write("Confirma el correo escribiendolo completo: ");
            var confirmEmail = Console.ReadLine()?.Trim();
            if (!string.Equals(confirmEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("Confirmacion incorrecta. No se modifico ningun dato.");
                return 2;
            }

            var password = ReadHiddenPassword("Nueva contraseña (14-128 caracteres): ");
            if (password.Length < AdminPasswordRecovery.MinimumPasswordLength
                || password.Length > AdminPasswordRecovery.MaximumPasswordLength)
            {
                Console.Error.WriteLine("Longitud invalida. No se modifico ningun dato.");
                return 2;
            }

            var confirmation = ReadHiddenPassword("Repite la nueva contraseña: ");
            if (!string.Equals(password, confirmation, StringComparison.Ordinal))
            {
                Console.Error.WriteLine("Las contraseñas no coinciden. No se modifico ningun dato.");
                return 2;
            }

            await AdminPasswordRecovery.ResetAsync(db, hasher, email, password);
            Console.WriteLine("Contraseña restablecida. Bloqueo eliminado y sesiones anteriores revocadas.");
            Console.WriteLine("Puedes iniciar sesión con la nueva contraseña.");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operación cancelada. No se modificó la contraseña.");
            return 2;
        }
        catch (Exception)
        {
            // No se registran errores SQL completos: podrían incluir cadenas de conexión.
            Console.Error.WriteLine("No se pudo restablecer la contraseña. Comprueba SQL Server y el estado de la cuenta.");
            return 1;
        }
    }

    private static string ReadHiddenPassword(string prompt)
    {
        Console.Write(prompt);
        var result = new StringBuilder();
        try
        {
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return result.ToString();
                }
                if (key.Key == ConsoleKey.Escape)
                    throw new OperationCanceledException();
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (result.Length > 0) result.Length--;
                    continue;
                }
                if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar)
                    && result.Length < AdminPasswordRecovery.MaximumPasswordLength + 1)
                    result.Append(key.KeyChar);
            }
        }
        finally { result.Clear(); }
    }
}
