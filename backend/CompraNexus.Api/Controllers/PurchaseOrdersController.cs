using CompraNexus.Api.Contracts;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public sealed class PurchaseOrdersController(CompraNexusDbContext db, AccountingGateway accounting) : ControllerBase
{
    private IQueryable<PurchaseOrder> Orders => db.PurchaseOrders.AsNoTracking()
        .Include(x => x.Department).Include(x => x.Employee).Include(x => x.Supplier)
        .Include(x => x.Lines).ThenInclude(x => x.Article)
        .Include(x => x.Lines).ThenInclude(x => x.UnitMeasure);

    private static PurchaseOrderView ToView(PurchaseOrder o) => new(
        o.Id, o.Number, o.OrderDate, o.Status, o.DepartmentId, o.Department.Name,
        o.EmployeeId, o.Employee.Name, o.SupplierId, o.Supplier.CommercialName,
        o.Lines.Sum(x => x.Quantity * x.UnitCost),
        o.Lines.Select(l => new PurchaseOrderLineView(l.Id, l.ArticleId, l.Article.Description,
            l.UnitMeasureId, l.UnitMeasure.Description, l.Quantity, l.UnitCost)).ToList());

    [HttpGet]
    public async Task<ActionResult<List<PurchaseOrderView>>> List(
        [FromQuery] int? departmentId, [FromQuery] int? employeeId,
        [FromQuery] string? status, [FromQuery] string? search)
    {
        var q = Orders;
        if (departmentId > 0) q = q.Where(x => x.DepartmentId == departmentId);
        if (employeeId > 0) q = q.Where(x => x.EmployeeId == employeeId);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Number.Contains(search));
        return Ok((await q.OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.Id).ToListAsync()).Select(ToView));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseOrderView>> Get(int id)
    {
        var order = await Orders.FirstOrDefaultAsync(x => x.Id == id);
        return order is null ? NotFound() : Ok(ToView(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderInput input)
    {
        if (!OrderRules.CanCreateAs(input.Status))
            return BadRequest(new { message = "Una orden nueva solo puede crearse como Borrador o Confirmada." });
        var error = await ValidateInput(input);
        if (error is not null) return BadRequest(new { message = error });
        var number = input.Number.Trim();
        if (await db.PurchaseOrders.AnyAsync(x => x.Number == number))
            return Conflict(new { message = "El número de orden ya existe." });
        var order = new PurchaseOrder();
        Apply(order, input);
        db.PurchaseOrders.Add(order);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "Número duplicado o datos relacionados inválidos." }); }
        return CreatedAtAction(nameof(Get), new { id = order.Id }, new { id = order.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderInput input)
    {
        var order = await db.PurchaseOrders.Include(x => x.Lines).Include(x => x.AccountingEntries)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        if (order.AccountingEntries.Count != 0) return Conflict(new { message = "La orden ya tiene asientos; no puede editarse." });
        if (!OrderRules.CanTransition(order.Status, input.Status))
            return Conflict(new { message = order.Status == "Anulada"
                ? "Una orden anulada no puede modificarse."
                : $"Transición de estado no permitida: {order.Status} → {input.Status}." });
        var error = await ValidateInput(input);
        if (error is not null) return BadRequest(new { message = error });
        var number = input.Number.Trim();
        if (await db.PurchaseOrders.AnyAsync(x => x.Number == number && x.Id != id))
            return Conflict(new { message = "El número de orden ya existe." });
        db.PurchaseOrderLines.RemoveRange(order.Lines);
        order.Lines.Clear();
        Apply(order, input);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "Número duplicado o datos relacionados inválidos." }); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.PurchaseOrders.Include(x => x.Lines).Include(x => x.AccountingEntries)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        if (order.Status != "Borrador" || order.AccountingEntries.Count != 0)
            return Conflict(new { message = "Solo se eliminan órdenes en borrador sin asientos." });
        db.PurchaseOrders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/accounting")]
    public async Task<IActionResult> GenerateAccounting(int id, CancellationToken ct)
    {
        var order = await db.PurchaseOrders.FindAsync([id], ct);
        if (order is null) return NotFound();
        int created;
        try { created = await accounting.PrepareAsync(order, ct); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        var entries = await db.AccountingEntries.Where(x => x.PurchaseOrderId == id && x.Status != "Enviado").ToListAsync(ct);
        var sent = await accounting.TransmitAsync(entries, ct);
        // Created = solo asientos nuevos de esta solicitud; Pending = los que siguen sin enviarse.
        return Ok(new AccountingResult(created, entries.Count - sent, sent, accounting.Configured));
    }

    private async Task<string?> ValidateInput(PurchaseOrderInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Number)) return "El número es obligatorio.";
        if (input.Lines is null || input.Lines.Count == 0) return "La orden requiere al menos un artículo.";
        if (input.Lines.Any(l => l.Quantity <= 0 || l.UnitCost < 0 || l.ArticleId <= 0))
            return "Cantidad y costo inválidos.";
        if (input.Lines.Any(l => decimal.Round(l.UnitCost, 2) != l.UnitCost))
            return "El costo unitario admite como máximo 2 decimales.";
        if (!await db.Departments.AnyAsync(d => d.Id == input.DepartmentId && d.Status == "Activo"))
            return "Departamento no válido.";
        if (!await db.Employees.AnyAsync(e => e.Id == input.EmployeeId && e.DepartmentId == input.DepartmentId && e.Status == "Activo"))
            return "El empleado no pertenece al departamento o está inactivo.";
        if (!await db.Suppliers.AnyAsync(s => s.Id == input.SupplierId && s.Status == "Activo"))
            return "Proveedor no válido.";
        var ids = input.Lines.Select(x => x.ArticleId).Distinct().ToArray();
        if (await db.Articles.CountAsync(a => ids.Contains(a.Id) && a.Status == "Activo") != ids.Length)
            return "Uno o más artículos no existen o están inactivos.";
        return null;
    }

    private void Apply(PurchaseOrder order, PurchaseOrderInput input)
    {
        order.Number = input.Number.Trim();
        order.OrderDate = input.OrderDate;
        order.Status = input.Status;
        order.DepartmentId = input.DepartmentId;
        order.EmployeeId = input.EmployeeId;
        order.SupplierId = input.SupplierId;
        var ids = input.Lines.Select(l => l.ArticleId).Distinct().ToArray();
        var articleUnits = db.Articles.AsNoTracking().Where(a => ids.Contains(a.Id))
            .ToDictionary(a => a.Id, a => a.UnitMeasureId);
        foreach (var line in input.Lines)
            order.Lines.Add(new PurchaseOrderLine
            {
                ArticleId = line.ArticleId, UnitMeasureId = articleUnits[line.ArticleId],
                Quantity = line.Quantity, UnitCost = line.UnitCost
            });
    }
}
