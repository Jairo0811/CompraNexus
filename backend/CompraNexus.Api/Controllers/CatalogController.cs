using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Controllers;

[ApiController]
public abstract class CatalogController<T>(CompraNexusDbContext db) : ControllerBase where T : class, ICatalogEntity
{
    [HttpGet]
    public async Task<ActionResult<List<T>>> All() => await db.Set<T>().AsNoTracking().OrderBy(x => x.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<T>> Get(int id)
    {
        var item = await db.Set<T>().FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<T>> Create([FromBody] T model)
    {
        model.Id = 0;
        if (!await ForeignKeysValid(model)) return BadRequest(new { message = "Referencia de catálogo inválida." });
        db.Set<T>().Add(model);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "Valor duplicado o referencia inválida." }); }
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] T model)
    {
        var existing = await db.Set<T>().FindAsync(id);
        if (existing is null) return NotFound();
        if (!await ForeignKeysValid(model)) return BadRequest(new { message = "Referencia de catálogo inválida." });
        model.Id = id;
        db.Entry(existing).CurrentValues.SetValues(model);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "Valor duplicado o referencia inválida." }); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await db.Set<T>().FindAsync(id);
        if (existing is null) return NotFound();
        existing.Status = "Inactivo";  // Soft delete preserves historical orders.
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ForeignKeysValid(T entity) => entity switch
    {
        Article x => x.UnitMeasureId > 0 && await db.UnitMeasures.AnyAsync(u => u.Id == x.UnitMeasureId),
        Employee x => x.DepartmentId > 0 && await db.Departments.AnyAsync(d => d.Id == x.DepartmentId),
        _ => true
    };
}

[Route("api/departments")]
public sealed class DepartmentsController(CompraNexusDbContext db) : CatalogController<Department>(db) { }

[Route("api/units")]
public sealed class UnitsController(CompraNexusDbContext db) : CatalogController<UnitMeasure>(db) { }

[Route("api/suppliers")]
public sealed class SuppliersController(CompraNexusDbContext db) : CatalogController<Supplier>(db) { }

[Route("api/articles")]
public sealed class ArticlesController(CompraNexusDbContext db) : CatalogController<Article>(db) { }

[Route("api/employees")]
public sealed class EmployeesController(CompraNexusDbContext db) : CatalogController<Employee>(db) { }
