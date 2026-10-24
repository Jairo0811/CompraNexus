using CompraNexus.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Data;

public static class DevelopmentSeed
{
    public static async Task RunAsync(CompraNexusDbContext db)
    {
        if (await db.Departments.AnyAsync()) return;
        var department = new Department { Name = "Tecnología" };
        var unit = new UnitMeasure { Description = "Unidad" };
        var supplier = new Supplier { TaxId = "101000001", CommercialName = "Proveedor de demostración" };
        db.AddRange(department, unit, supplier);
        await db.SaveChangesAsync();
        db.Employees.Add(new Employee { Name = "Empleado de demostración", DepartmentId = department.Id });
        db.Articles.Add(new Article { Description = "Laptop", Brand = "Genérica", UnitMeasureId = unit.Id, Stock = 10 });
        await db.SaveChangesAsync();
    }
}
