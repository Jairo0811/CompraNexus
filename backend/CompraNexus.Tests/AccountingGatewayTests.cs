using System.Net;
using CompraNexus.Api.Controllers;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using CompraNexus.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CompraNexus.Tests;

// Usan EF InMemory y un servicio contable SIMULADO (FakeAccountingHandler).
public class AccountingGatewayTests
{
    private sealed record Arrangement(CompraNexusDbContext Db, AccountingGateway Gateway, List<AccountingEntry> Entries);

    private static async Task<Arrangement> ArrangeAsync(FakeAccountingHandler handler, string? maxAttempts = null, bool configured = true)
    {
        var db = new CompraNexusDbContext(new DbContextOptionsBuilder<CompraNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var settings = new Dictionary<string, string?>
        {
            ["Accounting:InventoryTypeId"] = "1", ["Accounting:DebitAccount"] = "1401", ["Accounting:CreditAccount"] = "2101",
        };
        if (configured) settings["Accounting:ServiceUrl"] = "http://contabilidad-simulada.test/asientos";
        if (maxAttempts is not null) settings["Accounting:MaxAttempts"] = maxAttempts;
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var order = new PurchaseOrder
        {
            Number = "OC-3001", Status = "Confirmada", OrderDate = new DateOnly(2026, 10, 24),
            Lines = [new PurchaseOrderLine { ArticleId = 1, UnitMeasureId = 1, Quantity = 2, UnitCost = 75.25m }]
        };
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();
        var gateway = new AccountingGateway(new HttpClient(handler), config, db);
        await gateway.PrepareAsync(order, CancellationToken.None);
        return new Arrangement(db, gateway, await db.AccountingEntries.ToListAsync());
    }

    [Fact]
    public async Task Transmit_Success_SendsBalancedPayloadWithIdempotencyKey()
    {
        var handler = FakeAccountingHandler.Always(HttpStatusCode.OK);
        var a = await ArrangeAsync(handler);
        Assert.Equal(2, await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None));
        Assert.All(a.Entries, e => Assert.Equal(AccountingStatus.Sent, e.Status));
        Assert.Equal(2, handler.Calls);
        Assert.Equal(a.Entries.Select(e => e.JournalId.ToString()).Order(), handler.IdempotencyKeys.Order());
        Assert.All(handler.Bodies, b => Assert.Contains("\"montoAsiento\":150.5", b));
        Assert.Contains(handler.Bodies, b => b.Contains("\"tipoMovimiento\":\"DB\""));
        Assert.Contains(handler.Bodies, b => b.Contains("\"tipoMovimiento\":\"CR\""));
    }

    [Fact]
    public async Task Transmit_ServerError_IsRetriedWithSameIdempotencyKey()
    {
        var handler = new FakeAccountingHandler((_, call) =>
            new HttpResponseMessage(call <= 2 ? HttpStatusCode.InternalServerError : HttpStatusCode.OK));
        var a = await ArrangeAsync(handler);
        Assert.Equal(0, await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None));
        Assert.All(a.Entries, e => { Assert.Equal(AccountingStatus.Error, e.Status); Assert.Equal(1, e.Attempts); });

        Assert.Equal(2, await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None));
        Assert.All(a.Entries, e => { Assert.Equal(AccountingStatus.Sent, e.Status); Assert.Equal(2, e.Attempts); });
        // Reintento seguro: cada asiento se reenvió con la misma clave de idempotencia.
        Assert.Equal(4, handler.Calls);
        Assert.Equal(handler.IdempotencyKeys.Take(2).Order(), handler.IdempotencyKeys.Skip(2).Order());
    }

    [Fact]
    public async Task Transmit_Timeout_MarksErrorWithoutThrowing()
    {
        var a = await ArrangeAsync(FakeAccountingHandler.Timeout());
        Assert.Equal(0, await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None));
        Assert.All(a.Entries, e => { Assert.Equal(AccountingStatus.Error, e.Status); Assert.False(string.IsNullOrEmpty(e.Response)); });
    }

    [Fact]
    public async Task Transmit_ClientError_IsRejectedAndNeverRetried()
    {
        var handler = FakeAccountingHandler.Always(HttpStatusCode.BadRequest, "{\"error\":\"cuenta invalida\"}");
        var a = await ArrangeAsync(handler);
        await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None);
        await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None);
        Assert.All(a.Entries, e => Assert.Equal(AccountingStatus.Rejected, e.Status));
        Assert.Equal(2, handler.Calls);   // un solo intento por asiento
        Assert.Contains("cuenta invalida", a.Entries[0].Response);
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Transmit_TransientStatus_StaysRetryable(HttpStatusCode code)
    {
        var a = await ArrangeAsync(FakeAccountingHandler.Always(code));
        await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None);
        Assert.All(a.Entries, e => Assert.Equal(AccountingStatus.Error, e.Status));
    }

    [Fact]
    public async Task Transmit_StopsAfterMaxAttempts()
    {
        var handler = FakeAccountingHandler.Always(HttpStatusCode.ServiceUnavailable);
        var a = await ArrangeAsync(handler, maxAttempts: "2");
        for (var i = 0; i < 3; i++) await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None);
        Assert.Equal(4, handler.Calls);   // 2 asientos x 2 intentos
        Assert.All(a.Entries, e => Assert.Equal(2, e.Attempts));
    }

    [Fact]
    public async Task Transmit_WithoutServiceUrl_DoesNotCallHttp()
    {
        var handler = FakeAccountingHandler.Always(HttpStatusCode.OK);
        var a = await ArrangeAsync(handler, configured: false);
        Assert.Equal(0, await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None));
        Assert.Equal(0, handler.Calls);
        Assert.All(a.Entries, e => Assert.Equal(AccountingStatus.Pending, e.Status));
    }

    [Fact]
    public async Task Transmit_SkipsEntriesAlreadySent()
    {
        var handler = FakeAccountingHandler.Always(HttpStatusCode.OK);
        var a = await ArrangeAsync(handler);
        await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None);
        Assert.Equal(0, await a.Gateway.TransmitAsync(a.Entries, CancellationToken.None));
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task RetryEndpoint_IgnoresRejectedEntries()
    {
        var handler = FakeAccountingHandler.Always(HttpStatusCode.OK);
        var a = await ArrangeAsync(handler);
        a.Entries[0].Status = AccountingStatus.Rejected;
        await a.Db.SaveChangesAsync();

        var result = Assert.IsType<OkObjectResult>(await new AccountingController(a.Db, a.Gateway).Retry(CancellationToken.None));
        Assert.NotNull(result.Value);
        Assert.Equal(1, handler.Calls);   // solo el asiento pendiente se envió
        Assert.Equal(AccountingStatus.Rejected, a.Entries[0].Status);
        Assert.Equal(AccountingStatus.Sent, a.Entries[1].Status);
    }

    [Fact]
    public async Task RetryEndpoint_RequiresConfiguredService()
    {
        var a = await ArrangeAsync(FakeAccountingHandler.Always(HttpStatusCode.OK), configured: false);
        Assert.IsType<BadRequestObjectResult>(await new AccountingController(a.Db, a.Gateway).Retry(CancellationToken.None));
    }
}
