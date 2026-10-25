using CompraNexus.Api.Contracts;
using CompraNexus.Api.Controllers;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using CompraNexus.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CompraNexus.Tests;

// Pruebas con EF InMemory: validan reglas de controlador, NO demuestran persistencia en SQL Server
// ni el comportamiento de índices únicos (InMemory no los aplica).
public class OrderControllerTests
{
    private static CompraNexusDbContext NewDb() =>
        new(new DbContextOptionsBuilder<CompraNexusDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static PurchaseOrdersController NewController(CompraNexusDbContext db) =>
        new(db, new AccountingGateway(new HttpClient(), new ConfigurationBuilder().Build(), db));

    private static async Task SeedAsync(CompraNexusDbContext db)
    {
        var department = new Department { Name = "Tecnología" };
        var unit = new UnitMeasure { Description = "Unidad" };
        var supplier = new Supplier { TaxId = "101000001", CommercialName = "Proveedor" };
        db.AddRange(department, unit, supplier);
        await db.SaveChangesAsync();
        db.Employees.Add(new Employee { Name = "Empleado", DepartmentId = department.Id });
        db.Articles.Add(new Article { Description = "Laptop", Brand = "X", UnitMeasureId = unit.Id, Stock = 5 });
        await db.SaveChangesAsync();
    }

    private static PurchaseOrderInput Input(CompraNexusDbContext db, string number = "OC-1",
        string status = "Borrador", decimal cost = 1500m, int quantity = 2) => new()
    {
        Number = number, Status = status,
        DepartmentId = db.Departments.Single().Id, EmployeeId = db.Employees.Single().Id,
        SupplierId = db.Suppliers.Single().Id,
        Lines = [new PurchaseOrderLineInput { ArticleId = db.Articles.Single().Id, Quantity = quantity, UnitCost = cost }]
    };

    [Theory]
    [InlineData("Borrador", "Borrador", true)]
    [InlineData("Borrador", "Confirmada", true)]
    [InlineData("Borrador", "Anulada", true)]
    [InlineData("Confirmada", "Confirmada", true)]
    [InlineData("Confirmada", "Anulada", true)]
    [InlineData("Confirmada", "Borrador", false)]
    [InlineData("Anulada", "Borrador", false)]
    [InlineData("Anulada", "Confirmada", false)]
    [InlineData("Anulada", "Anulada", false)]
    public void OrderRules_Transitions(string from, string to, bool allowed) =>
        Assert.Equal(allowed, OrderRules.CanTransition(from, to));

    [Fact]
    public async Task Create_RejectsNewOrderAsAnulada()
    {
        await using var db = NewDb(); await SeedAsync(db);
        Assert.IsType<BadRequestObjectResult>(await NewController(db).Create(Input(db, status: "Anulada")));
        Assert.Empty(db.PurchaseOrders);
    }

    [Fact]
    public async Task Create_RejectsCostWithMoreThanTwoDecimals()
    {
        await using var db = NewDb(); await SeedAsync(db);
        Assert.IsType<BadRequestObjectResult>(await NewController(db).Create(Input(db, cost: 10.555m)));
        Assert.Empty(db.PurchaseOrders);
    }

    [Fact]
    public async Task Create_DetectsDuplicateNumberIgnoringSurroundingSpaces()
    {
        await using var db = NewDb(); await SeedAsync(db);
        var controller = NewController(db);
        Assert.IsType<CreatedAtActionResult>(await controller.Create(Input(db, "OC-9")));
        Assert.IsType<ConflictObjectResult>(await controller.Create(Input(db, " OC-9 ")));
        Assert.Single(db.PurchaseOrders);
    }

    [Fact]
    public async Task Update_RejectsConfirmadaBackToBorrador()
    {
        await using var db = NewDb(); await SeedAsync(db);
        var controller = NewController(db);
        await controller.Create(Input(db, status: "Confirmada"));
        var id = db.PurchaseOrders.Single().Id;
        Assert.IsType<ConflictObjectResult>(await controller.Update(id, Input(db, status: "Borrador")));
    }

    [Fact]
    public async Task Update_AnuladaIsFinal()
    {
        await using var db = NewDb(); await SeedAsync(db);
        var controller = NewController(db);
        await controller.Create(Input(db));
        var id = db.PurchaseOrders.Single().Id;
        Assert.IsType<NoContentResult>(await controller.Update(id, Input(db, status: "Anulada")));
        Assert.IsType<ConflictObjectResult>(await controller.Update(id, Input(db, status: "Anulada")));
    }

    [Fact]
    public async Task Update_AllowsBorradorToConfirmada()
    {
        await using var db = NewDb(); await SeedAsync(db);
        var controller = NewController(db);
        await controller.Create(Input(db));
        var id = db.PurchaseOrders.Single().Id;
        Assert.IsType<NoContentResult>(await controller.Update(id, Input(db, status: "Confirmada")));
        Assert.Equal("Confirmada", db.PurchaseOrders.Single().Status);
    }

    [Fact]
    public async Task GenerateAccounting_CreatedCountsOnlyNewEntries()
    {
        await using var db = NewDb(); await SeedAsync(db);
        var controller = NewController(db);
        await controller.Create(Input(db, status: "Confirmada"));
        var id = db.PurchaseOrders.Single().Id;

        var first = Assert.IsType<AccountingResult>(Assert.IsType<OkObjectResult>(
            await controller.GenerateAccounting(id, CancellationToken.None)).Value);
        Assert.Equal(2, first.Created);   // DB + CR nuevos
        Assert.Equal(2, first.Pending);   // sin servicio configurado quedan pendientes
        Assert.False(first.Configured);

        var second = Assert.IsType<AccountingResult>(Assert.IsType<OkObjectResult>(
            await controller.GenerateAccounting(id, CancellationToken.None)).Value);
        Assert.Equal(0, second.Created);  // los pendientes existentes NO cuentan como creados
        Assert.Equal(2, second.Pending);
        Assert.Equal(2, db.AccountingEntries.Count());
    }

    [Fact]
    public async Task GenerateAccounting_RejectsDraftOrder()
    {
        await using var db = NewDb(); await SeedAsync(db);
        var controller = NewController(db);
        await controller.Create(Input(db));
        var id = db.PurchaseOrders.Single().Id;
        Assert.IsType<BadRequestObjectResult>(await controller.GenerateAccounting(id, CancellationToken.None));
        Assert.Empty(db.AccountingEntries);
    }
}
