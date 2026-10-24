using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CompraNexus.Tests;

public class DomainTests
{
    [Fact]
    public void PurchaseOrderLines_AreLinkedToArticlesAndUnits()
    {
        var o = new PurchaseOrder
        {
            Number = "OC-0001", Status = "Borrador",
            Lines = [new PurchaseOrderLine { ArticleId = 10, UnitMeasureId = 4, Quantity = 3, UnitCost = 650.50m }]
        };
        Assert.Equal(1951.50m, o.Lines.Sum(x => x.Quantity * x.UnitCost));
        Assert.Equal("Borrador", o.Status);
    }

    [Fact]
    public async Task AccountingOutbox_CreatesBalancedEntriesWithoutDuplicates()
    {
        var options = new DbContextOptionsBuilder<CompraNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new CompraNexusDbContext(options);
        var order = new PurchaseOrder
        {
            Number = "OC-1001", Status = "Confirmada",
            OrderDate = new DateOnly(2026, 10, 24),
            Lines = [new PurchaseOrderLine { ArticleId = 1, UnitMeasureId = 1, Quantity = 2, UnitCost = 75.25m }]
        };
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["Accounting:InventoryTypeId"] = "1",
            ["Accounting:DebitAccount"] = "1401",
            ["Accounting:CreditAccount"] = "2101",
        }).Build();
        var gateway = new AccountingGateway(new HttpClient(), config, db);
        Assert.Equal(2, await gateway.PrepareAsync(order, CancellationToken.None));
        Assert.Equal(0, await gateway.PrepareAsync(order, CancellationToken.None));
        var entries = await db.AccountingEntries.ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.All(entries, x => Assert.Equal(150.50m, x.Amount));
        Assert.Equal(new[] { "CR", "DB" }, entries.Select(x => x.Movement).Order().ToArray());
        Assert.All(entries, x => Assert.Equal("Pendiente", x.Status));
        Assert.False(gateway.Configured);
    }

    [Fact]
    public async Task DraftOrder_IsNotPostedToAccounting()
    {
        var options = new DbContextOptionsBuilder<CompraNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new CompraNexusDbContext(options);
        var order = new PurchaseOrder { Number = "OC-0002", Status = "Borrador" };
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();
        var gateway = new AccountingGateway(new HttpClient(), new ConfigurationBuilder().Build(), db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => gateway.PrepareAsync(order, CancellationToken.None));
        Assert.Empty(await db.AccountingEntries.ToListAsync());
    }
}
