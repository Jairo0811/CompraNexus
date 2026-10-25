using CompraNexus.Api.Contracts;
using CompraNexus.Api.Controllers;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CompraNexus.Tests.SqlServer;

// Estas pruebas SÍ usan SQL Server real (base temporal). No prueban el servicio contable externo.
public class SqlServerIntegrationTests : SqlServerTestBase
{
    private const string IndexName = "IX_AccountingEntries_PurchaseOrderId_Movement";

    private static AccountingGateway Gateway(CompraNexusDbContext db) =>
        new(new HttpClient(), new ConfigurationBuilder().Build(), db);

    private static PurchaseOrdersController Orders(CompraNexusDbContext db) => new(db, Gateway(db));

    private static PurchaseOrderInput Input(Seed s, string number, string status, params (int Quantity, decimal Cost)[] lines) => new()
    {
        Number = number, Status = status, DepartmentId = s.DepartmentId, EmployeeId = s.EmployeeId, SupplierId = s.SupplierId,
        Lines = lines.Select(l => new PurchaseOrderLineInput { ArticleId = s.ArticleId, Quantity = l.Quantity, UnitCost = l.Cost }).ToList()
    };

    private static Task<int> UniqueIndexCount(CompraNexusDbContext db) =>
        db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS [Value] FROM sys.indexes WHERE name = N'" + IndexName +
            "' AND is_unique = 1 AND object_id = OBJECT_ID(N'dbo.AccountingEntries')").SingleAsync();

    private static Task DropIndex(CompraNexusDbContext db) =>
        db.Database.ExecuteSqlRawAsync("DROP INDEX " + IndexName + " ON dbo.AccountingEntries");

    [SqlServerFact]
    public async Task Schema_CreatesUniqueIndexOnAccountingEntries()
    {
        await using var db = NewDb();
        Assert.Equal(1, await UniqueIndexCount(db));
    }

    [SqlServerFact]
    public async Task UniqueIndex_RejectsSecondEntryWithSameOrderAndMovement()
    {
        await using var db = NewDb();
        var order = await AddOrderAsync(db, await SeedAsync(db), "OC-IDX", "Confirmada", 2, 75.25m);
        Assert.Equal(2, await Gateway(db).PrepareAsync(order, CancellationToken.None));

        db.AccountingEntries.Add(new AccountingEntry
        {
            PurchaseOrderId = order.Id, Movement = "DB", Account = "1401", InventoryTypeId = "1",
            Description = "duplicado", EntryDate = order.OrderDate, Amount = 150.50m
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [SqlServerFact]
    public async Task SchemaPatch_RecreatesMissingIndex_KeepingData()
    {
        await using var db = NewDb();
        var order = await AddOrderAsync(db, await SeedAsync(db), "OC-PATCH", "Confirmada", 2, 75.25m);
        await Gateway(db).PrepareAsync(order, CancellationToken.None);
        await DropIndex(db);
        Assert.Equal(0, await UniqueIndexCount(db));

        await SchemaPatches.ApplyAsync(db, NullLogger.Instance);

        Assert.Equal(1, await UniqueIndexCount(db));
        Assert.Equal(2, await db.AccountingEntries.CountAsync());
    }

    [SqlServerFact]
    public async Task SchemaPatch_WithPreexistingDuplicates_DoesNotFailNorDeleteData()
    {
        await using var db = NewDb();
        var order = await AddOrderAsync(db, await SeedAsync(db), "OC-DUP", "Confirmada", 2, 75.25m);
        await Gateway(db).PrepareAsync(order, CancellationToken.None);
        await DropIndex(db);
        db.AccountingEntries.Add(new AccountingEntry
        {
            PurchaseOrderId = order.Id, Movement = "DB", Account = "1401", InventoryTypeId = "1",
            Description = "duplicado previo", EntryDate = order.OrderDate, Amount = 150.50m
        });
        await db.SaveChangesAsync();

        await SchemaPatches.ApplyAsync(db, NullLogger.Instance);

        Assert.Equal(0, await UniqueIndexCount(db));              // no se crea con duplicados
        Assert.Equal(3, await db.AccountingEntries.CountAsync()); // y no se borra nada
    }

    [SqlServerFact]
    public async Task Prepare_UnderConcurrency_CreatesExactlyOnePair()
    {
        int orderId;
        await using (var setup = NewDb())
            orderId = (await AddOrderAsync(setup, await SeedAsync(setup), "OC-CONC", "Confirmada", 2, 75.25m)).Id;

        var start = new TaskCompletionSource();
        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(async () =>
        {
            await using var db = NewDb();
            var order = await db.PurchaseOrders.AsNoTracking().SingleAsync(x => x.Id == orderId);
            var gateway = Gateway(db);
            await start.Task;
            return await gateway.PrepareAsync(order, CancellationToken.None);
        })).ToList();
        start.SetResult();
        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.Contains(r, new[] { 0, 2 }));
        Assert.Equal(2, results.Sum());   // exactamente una solicitud creó el par DB/CR

        await using var verify = NewDb();
        var entries = await verify.AccountingEntries.AsNoTracking().OrderBy(e => e.Movement).ToListAsync();
        Assert.Equal(new[] { "CR", "DB" }, entries.Select(e => e.Movement).ToArray());
        Assert.All(entries, e => Assert.Equal(150.50m, e.Amount));
    }

    [SqlServerFact]
    public async Task Order_PersistsLinesAndTotal_AndRejectsDuplicateNumber()
    {
        await using var db = NewDb();
        var seed = await SeedAsync(db);
        Assert.IsType<CreatedAtActionResult>(await Orders(db).Create(Input(seed, "OC-100", "Borrador", (2, 1500m), (3, 250.50m))));

        await using var verify = NewDb();
        var order = await verify.PurchaseOrders.Include(o => o.Lines).SingleAsync();
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(3751.50m, await verify.PurchaseOrderLines.SumAsync(l => l.Quantity * l.UnitCost));

        Assert.IsType<ConflictObjectResult>(await Orders(db).Create(Input(seed, "OC-100", "Borrador", (1, 10m))));
        verify.PurchaseOrders.Add(new PurchaseOrder
        {
            Number = "OC-100", DepartmentId = seed.DepartmentId, EmployeeId = seed.EmployeeId, SupplierId = seed.SupplierId
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => verify.SaveChangesAsync());   // índice único en la base
    }

    [SqlServerFact]
    public async Task Update_ReplacesLines_WithoutLeavingOrphans()
    {
        await using var db = NewDb();
        var seed = await SeedAsync(db);
        await Orders(db).Create(Input(seed, "OC-UPD", "Borrador", (2, 1500m), (3, 250.50m)));
        var id = (await db.PurchaseOrders.AsNoTracking().SingleAsync()).Id;

        Assert.IsType<NoContentResult>(await Orders(db).Update(id, Input(seed, "OC-UPD", "Borrador", (5, 100m))));

        await using var verify = NewDb();
        Assert.Equal(1, await verify.PurchaseOrderLines.CountAsync());
        Assert.Equal(500m, await verify.PurchaseOrderLines.SumAsync(l => l.Quantity * l.UnitCost));
    }

    [SqlServerFact]
    public async Task Delete_RemovesDraftWithItsLines_ButKeepsConfirmedOrders()
    {
        await using var db = NewDb();
        var seed = await SeedAsync(db);
        await Orders(db).Create(Input(seed, "OC-DRAFT", "Borrador", (1, 100m)));
        await Orders(db).Create(Input(seed, "OC-CONF", "Confirmada", (1, 100m)));
        var draftId = (await db.PurchaseOrders.AsNoTracking().SingleAsync(o => o.Number == "OC-DRAFT")).Id;
        var confirmedId = (await db.PurchaseOrders.AsNoTracking().SingleAsync(o => o.Number == "OC-CONF")).Id;

        Assert.IsType<NoContentResult>(await Orders(db).Delete(draftId));
        Assert.IsType<ConflictObjectResult>(await Orders(db).Delete(confirmedId));

        await using var verify = NewDb();
        Assert.Equal(new[] { "OC-CONF" }, await verify.PurchaseOrders.Select(o => o.Number).ToArrayAsync());
        Assert.Equal(1, await verify.PurchaseOrderLines.CountAsync());   // las líneas del borrador se eliminaron en cascada
    }

    [SqlServerFact]
    public async Task GenerateAccounting_PersistsOneBalancedPair_AcrossRequests()
    {
        await using var db = NewDb();
        var seed = await SeedAsync(db);
        await Orders(db).Create(Input(seed, "OC-ACC", "Confirmada", (2, 1500m), (1, 250.50m)));
        var id = (await db.PurchaseOrders.AsNoTracking().SingleAsync()).Id;

        await using var first = NewDb();
        var r1 = Assert.IsType<AccountingResult>(Assert.IsType<OkObjectResult>(
            await Orders(first).GenerateAccounting(id, CancellationToken.None)).Value);
        await using var second = NewDb();
        var r2 = Assert.IsType<AccountingResult>(Assert.IsType<OkObjectResult>(
            await Orders(second).GenerateAccounting(id, CancellationToken.None)).Value);

        Assert.Equal(2, r1.Created);
        Assert.Equal(0, r2.Created);
        Assert.Equal(2, r2.Pending);

        await using var verify = NewDb();
        var entries = await verify.AccountingEntries.AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(2, entries.Select(e => e.JournalId).Distinct().Count());
        Assert.Equal(entries.Single(e => e.Movement == "DB").Amount, entries.Single(e => e.Movement == "CR").Amount);
        Assert.Equal(3250.50m, entries[0].Amount);
        Assert.All(entries, e => Assert.Equal(AccountingStatus.Pending, e.Status));
    }

    [SqlServerFact]
    public async Task ForeignKeys_PreventDeletingReferencedDepartment()
    {
        // Sembrar y cerrar el contexto: un dependiente rastreado puede provocar
        // InvalidOperationException en EF Core ANTES de ejecutar el DELETE SQL.
        await using (var setup = NewDb())
            await SeedAsync(setup);

        // Este contexto solo rastrea al Department: el servidor debe rechazar
        // el DELETE debido a la FK real de Employees (SQL Server error 547).
        await using var delete = NewDb();
        delete.Departments.Remove(await delete.Departments.SingleAsync());
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => delete.SaveChangesAsync());
        Assert.Equal(547, Assert.IsType<Microsoft.Data.SqlClient.SqlException>(error.InnerException).Number);

        await using var verify = NewDb();
        Assert.Equal(1, await verify.Departments.CountAsync());
        Assert.Equal(1, await verify.Employees.CountAsync());
    }
}
