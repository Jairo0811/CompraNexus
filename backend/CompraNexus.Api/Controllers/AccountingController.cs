using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Controllers;

[ApiController]
[Route("api/accounting")]
public sealed class AccountingController(CompraNexusDbContext db, IAccountingGateway gateway) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(new
    {
        configured = gateway.Configured,
        entries = await db.AccountingEntries.AsNoTracking().OrderByDescending(a => a.Id)
            .Select(a => new
            {
                a.Id, a.JournalId, a.PurchaseOrderId, a.Description, a.InventoryTypeId,
                a.Account, a.Movement, a.EntryDate, a.Amount, a.Status, a.Attempts
            }).ToListAsync()
    });

    [HttpPost("retry")]
    public async Task<IActionResult> Retry(CancellationToken ct)
    {
        if (!gateway.Configured) return BadRequest(new { message = "Configure Accounting:ServiceUrl primero." });
        // Solo Pendiente/Error; los Rechazado (4xx) requieren revisión manual y no se reenvían.
        var entries = await db.AccountingEntries
            .Where(a => a.Status == AccountingStatus.Pending || a.Status == AccountingStatus.Error).ToListAsync(ct);
        var sent = await gateway.TransmitAsync(entries, ct);
        var rejected = await db.AccountingEntries.CountAsync(a => a.Status == AccountingStatus.Rejected, ct);
        return Ok(new { sent, remaining = entries.Count(a => a.Status != AccountingStatus.Sent), rejected });
    }
}

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(CompraNexusDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(new
    {
        departments = await db.Departments.CountAsync(x => x.Status == "Activo"),
        articles = await db.Articles.CountAsync(x => x.Status == "Activo"),
        suppliers = await db.Suppliers.CountAsync(x => x.Status == "Activo"),
        orders = await db.PurchaseOrders.CountAsync(),
    });
}
