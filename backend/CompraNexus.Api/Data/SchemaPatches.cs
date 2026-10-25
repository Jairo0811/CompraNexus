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

    public static async Task ApplyAsync(CompraNexusDbContext db, ILogger logger)
    {
        try { await db.Database.ExecuteSqlRawAsync(AccountingUniqueIndexSql); }
        catch (Exception ex) { logger.LogWarning(ex, "No se pudo aplicar el índice único de asientos; revise scripts/sql/001-accounting-unique-index.sql."); }
    }
}
