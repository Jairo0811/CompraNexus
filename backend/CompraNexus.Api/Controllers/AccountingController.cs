using CompraNexus.Api.Data;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Controllers;

[ApiController]
[Route("api/accounting")]
public sealed class AccountingController(CompraNexusDbContext db, AccountingGateway gateway) : ControllerBase
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
        var entries = await db.AccountingEntries.Where(a => a.Status != "Enviado").ToListAsync(ct);
        var sent = await gateway.TransmitAsync(entries, ct);
        return Ok(new { sent, remaining = entries.Count - sent });
    }
}

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(CompraNexusDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(new
    {
        departments = await db.Departments.CountAsync(),
        articles = await db.Articles.CountAsync(),
        suppliers = await db.Suppliers.CountAsync(),
        orders = await db.PurchaseOrders.CountAsync(),
    });
}
