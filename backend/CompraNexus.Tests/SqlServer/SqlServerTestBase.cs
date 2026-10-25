using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CompraNexus.Tests.SqlServer;

public sealed record Seed(int DepartmentId, int EmployeeId, int SupplierId, int ArticleId, int UnitId);

/// <summary>Cada prueba obtiene una base temporal propia, creada con el modelo real de EF Core (EnsureCreated).</summary>
public abstract class SqlServerTestBase : IAsyncLifetime
{
    private const string Prefix = "CompraNexus_Test_";
    protected string DatabaseName { get; } = Prefix + Guid.NewGuid().ToString("N");
    protected string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        if (!SqlServerFactAttribute.Enabled) return;
        var builder = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable(SqlServerFactAttribute.VariableName)!)
        {
            InitialCatalog = DatabaseName   // se sobrescribe cualquier base indicada: nunca se usa «CompraNexus»
        };
        ConnectionString = builder.ConnectionString;
        await using var db = NewDb();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (ConnectionString.Length == 0 || !DatabaseName.StartsWith(Prefix, StringComparison.Ordinal)) return;
        await using var db = NewDb();
        await db.Database.EnsureDeletedAsync();
    }

    protected CompraNexusDbContext NewDb() =>
        new(new DbContextOptionsBuilder<CompraNexusDbContext>().UseSqlServer(ConnectionString).Options);

    protected static async Task<Seed> SeedAsync(CompraNexusDbContext db)
    {
        var department = new Department { Name = "Tecnología" };
        var unit = new UnitMeasure { Description = "Unidad" };
        var supplier = new Supplier { TaxId = "101000001", CommercialName = "Proveedor Prueba" };
        db.AddRange(department, unit, supplier);
        await db.SaveChangesAsync();
        var employee = new Employee { Name = "Empleado Prueba", DepartmentId = department.Id };
        var article = new Article { Description = "Laptop", Brand = "Marca", UnitMeasureId = unit.Id, Stock = 10 };
        db.AddRange(employee, article);
        await db.SaveChangesAsync();
        return new Seed(department.Id, employee.Id, supplier.Id, article.Id, unit.Id);
    }

    protected static async Task<PurchaseOrder> AddOrderAsync(
        CompraNexusDbContext db, Seed s, string number, string status, int quantity, decimal cost)
    {
        var order = new PurchaseOrder
        {
            Number = number, Status = status, OrderDate = new DateOnly(2026, 10, 24),
            DepartmentId = s.DepartmentId, EmployeeId = s.EmployeeId, SupplierId = s.SupplierId,
            Lines = [new PurchaseOrderLine { ArticleId = s.ArticleId, UnitMeasureId = s.UnitId, Quantity = quantity, UnitCost = cost }]
        };
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }
}
