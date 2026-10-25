using System.Net.Http.Headers;
using System.Net.Http.Json;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Services;

public sealed class AccountingGateway(
    HttpClient client, IConfiguration configuration, CompraNexusDbContext db) : IAccountingGateway
{
    private const int DefaultMaxAttempts = 5;
    // Serializa envíos dentro del proceso; entre procesos protege la Idempotency-Key (JournalId).
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private readonly string? url = configuration["Accounting:ServiceUrl"];
    public bool Configured => !string.IsNullOrWhiteSpace(url);

    private int MaxAttempts =>
        int.TryParse(configuration["Accounting:MaxAttempts"], out var n) && n > 0 ? n : DefaultMaxAttempts;

    private static string Truncate(string text) => text[..Math.Min(text.Length, 1900)];

    /// <summary>Envía los asientos reintentables. Un 4xx (salvo 408/429) pasa a Rechazado y no se reintenta;
    /// 5xx, timeout y errores de red quedan en Error y se reintentan hasta MaxAttempts con la misma Idempotency-Key.</summary>
    public async Task<int> TransmitAsync(IEnumerable<AccountingEntry> entries, CancellationToken ct)
    {
        if (!Configured) return 0;
        var delivered = 0;
        await Gate.WaitAsync(ct);
        try
        {
            foreach (var entry in entries)
            {
                // Estado fresco: otra solicitud pudo enviarlo mientras esperábamos el turno.
                await db.Entry(entry).ReloadAsync(ct);
                if (!AccountingStatus.IsRetryable(entry.Status, entry.Attempts, MaxAttempts)) continue;
                var payload = new
                {
                    identificadorAsiento = entry.JournalId,
                    descripcion = entry.Description,
                    identificadorTipoInventario = entry.InventoryTypeId,
                    cuentaContable = entry.Account,
                    tipoMovimiento = entry.Movement,
                    fechaAsiento = entry.EntryDate.ToString("yyyy-MM-dd"),
                    montoAsiento = entry.Amount,
                    estado = "Activo"
                };
                entry.Attempts++;
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Post, url);
                    req.Headers.TryAddWithoutValidation("Idempotency-Key", entry.JournalId.ToString());
                    if (!string.IsNullOrEmpty(configuration["Accounting:BearerToken"]))
                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration["Accounting:BearerToken"]);
                    req.Content = JsonContent.Create(payload);
                    using var res = await client.SendAsync(req, ct);
                    entry.Response = Truncate(await res.Content.ReadAsStringAsync(ct));
                    var code = (int)res.StatusCode;
                    if (res.IsSuccessStatusCode) { entry.Status = AccountingStatus.Sent; delivered++; }
                    else if (code is >= 400 and < 500 and not 408 and not 429) entry.Status = AccountingStatus.Rejected;
                    else entry.Status = AccountingStatus.Error;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    // Timeout del HttpClient, red caída, URL inválida, etc.
                    entry.Status = AccountingStatus.Error;
                    entry.Response = Truncate(ex.Message);
                }
                await db.SaveChangesAsync(ct);
            }
        }
        finally { Gate.Release(); }
        return delivered;
    }

    public async Task<int> PrepareAsync(PurchaseOrder order, CancellationToken ct)
    {
        if (order.Status != "Confirmada") throw new InvalidOperationException("Solo se contabilizan órdenes confirmadas.");
        if (await db.AccountingEntries.AnyAsync(a => a.PurchaseOrderId == order.Id, ct)) return 0;
        var total = await db.PurchaseOrderLines.Where(x => x.PurchaseOrderId == order.Id)
            .SumAsync(x => x.Quantity * x.UnitCost, ct);
        if (total <= 0) throw new InvalidOperationException("El total debe ser mayor que cero.");
        var inventory = configuration["Accounting:InventoryTypeId"] ?? "1";
        var debitAccount = configuration["Accounting:DebitAccount"] ?? "1401";
        var creditAccount = configuration["Accounting:CreditAccount"] ?? "2101";
        foreach (var (movement, account) in new[] { ("DB", debitAccount), ("CR", creditAccount) })
            db.AccountingEntries.Add(new AccountingEntry
            {
                PurchaseOrderId = order.Id,
                Description = "Orden de compra " + order.Number,
                InventoryTypeId = inventory,
                Account = account,
                Movement = movement,
                EntryDate = order.OrderDate,
                Amount = total
            });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException)
        {
            // Otra solicitud concurrente pudo crear el par DB/CR primero (índice único por orden y movimiento).
            db.ChangeTracker.Clear();
            if (await db.AccountingEntries.AsNoTracking().AnyAsync(a => a.PurchaseOrderId == order.Id, ct)) return 0;
            throw;
        }
        return 2;
    }
}
