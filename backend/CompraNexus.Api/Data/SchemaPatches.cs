using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Data;

/// <summary>
/// Ajustes aditivos e idempotentes para bases creadas con EnsureCreated (sin historial de migraciones).
/// No borra ni modifica datos. Equivale a scripts/sql/001-accounting-unique-index.sql.
/// </summary>
public static class SchemaPatches
{
    public const string AccountingUniqueIndexSql = """
        IF OBJECT_ID(N'dbo.AccountingEntries', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.indexes
                           WHERE name = N'IX_AccountingEntries_PurchaseOrderId_Movement'
                             AND object_id = OBJECT_ID(N'dbo.AccountingEntries'))
           AND NOT EXISTS (SELECT 1 FROM dbo.AccountingEntries
                           GROUP BY PurchaseOrderId, Movement HAVING COUNT(*) > 1)
            CREATE UNIQUE INDEX IX_AccountingEntries_PurchaseOrderId_Movement
                ON dbo.AccountingEntries (PurchaseOrderId, Movement);
        """;

    // Tabla exclusivamente aditiva para bases antiguas creadas con EnsureCreated.
    // No toca los datos de compras ni reemplaza una migración EF Core completa.
    public const string AppUsersSql = """
        IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.AppUsers (
                Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
                Email nvarchar(180) NOT NULL,
                NormalizedEmail nvarchar(180) NOT NULL,
                PasswordHash nvarchar(512) NOT NULL,
                Role nvarchar(40) NOT NULL,
                IsActive bit NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT (1),
                FailedAttempts int NOT NULL CONSTRAINT DF_AppUsers_FailedAttempts DEFAULT (0),
                LockoutEndUtc datetimeoffset(7) NULL,
                SecurityStamp nvarchar(64) NOT NULL
            );
        END;
        IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.indexes
                           WHERE name = N'IX_AppUsers_NormalizedEmail'
                             AND object_id = OBJECT_ID(N'dbo.AppUsers'))
            CREATE UNIQUE INDEX IX_AppUsers_NormalizedEmail ON dbo.AppUsers(NormalizedEmail);
        """;

    public static async Task ApplyAsync(CompraNexusDbContext db, ILogger logger)
    {
        try { await db.Database.ExecuteSqlRawAsync(AccountingUniqueIndexSql); }
        catch (Exception ex) { logger.LogWarning(ex, "No se pudo aplicar el índice único de asientos; revise scripts/sql/001-accounting-unique-index.sql."); }
        // La autenticación debe fallar cerrada si la tabla de usuarios no puede crearse.
        await db.Database.ExecuteSqlRawAsync(AppUsersSql);
    }
}
