using System.Net.Http.Headers;
using System.Net.Http.Json;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Services;

public sealed class AccountingGateway(
    HttpClient client, IConfiguration configuration, CompraNexusDbContext db)
{
    private readonly string? url = configuration["Accounting:ServiceUrl"];
    public bool Configured => !string.IsNullOrWhiteSpace(url);

    public async Task<int> TransmitAsync(IEnumerable<AccountingEntry> entries, CancellationToken ct)
    {
        if (!Configured) return 0;
        var delivered = 0;
        foreach (var entry in entries)
        {
            if (entry.Status == "Enviado") continue;
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
                var responseText = await res.Content.ReadAsStringAsync(ct);
                entry.Response = responseText[..Math.Min(responseText.Length, 1900)];
                if (res.IsSuccessStatusCode) { entry.Status = "Enviado"; delivered++; }
                else entry.Status = "Error";
            }
            catch (HttpRequestException ex) { entry.Status = "Error"; entry.Response = ex.Message[..Math.Min(ex.Message.Length, 1900)]; }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested) { entry.Status = "Error"; entry.Response = ex.Message[..Math.Min(ex.Message.Length, 1900)]; }
            await db.SaveChangesAsync(ct);
        }
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
        await db.SaveChangesAsync(ct);
        return 2;
    }
}
