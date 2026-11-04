using CompraNexus.Api.Controllers;
using CompraNexus.Api.Data;
using CompraNexus.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CompraNexus.Tests;

public class DominicanTaxIdTests
{
    [Theory]
    [InlineData("101000007", "rnc", "101000007")]
    [InlineData("1-0100000-7", "rnc", "101000007")]
    [InlineData("00100000009", "cedula", "00100000009")]
    [InlineData("001-0000000-9", "cedula", "00100000009")]
    public void AcceptsValidChecksumsAndNormalizes(string input, string type, string expected)
    {
        var result = DominicanTaxId.Check(input);
        Assert.True(result.IsValid);
        Assert.Equal(type, result.Type);
        Assert.Equal(expected, result.Normalized);
        Assert.Contains("No confirma", result.Message);
    }

    [Theory]
    [InlineData("101000001")]
    [InlineData("00100000000")]
    [InlineData("00000000000")]
    [InlineData("000000000")]
    [InlineData("123")]
    [InlineData("123456789ABC")]
    [InlineData("1/0100000/7")]
    public void RejectsInvalidOrMalformedIdentifiers(string input) =>
        Assert.False(DominicanTaxId.Check(input).IsValid);

    [Fact]
    public void IdentificationPreviewNeverClaimsRegistryVerification()
    {
        var result = new IdentificationController().Validate(new IdentificationInput("101000007"));
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var validation = Assert.IsType<TaxIdValidation>(ok.Value);
        Assert.True(validation.IsValid);
        Assert.Contains("DGII", validation.Message);
    }

    private static CompraNexusDbContext NewDb() => new(
        new DbContextOptionsBuilder<CompraNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options);

    [Fact]
    public async Task SupplierApiRejectsInvalidRncWithoutSaving()
    {
        await using var db = NewDb();
        var response = await new SuppliersController(db).Create(
            new Supplier { TaxId = "101000001", CommercialName = "Prueba" });
        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Empty(await db.Suppliers.ToListAsync());
    }

    [Fact]
    public async Task SupplierApiNormalizesBothKindsOnCreateAndUpdate()
    {
        await using var db = NewDb();
        var controller = new SuppliersController(db);
        var newSupplier = new Supplier { TaxId = "1-0100000-7", CommercialName = "Prueba" };
        Assert.IsType<CreatedAtActionResult>((await controller.Create(newSupplier)).Result);
        Assert.Equal("101000007", (await db.Suppliers.SingleAsync()).TaxId);
        Assert.IsType<NoContentResult>(await controller.Update(newSupplier.Id,
            new Supplier { TaxId = "001-0000000-9", CommercialName = "Editada" }));
        Assert.Equal("00100000009", (await db.Suppliers.SingleAsync()).TaxId);
    }

    [Fact]
    public async Task SupplierApiInvalidUpdateKeepsExistingSupplier()
    {
        await using var db = NewDb();
        var controller = new SuppliersController(db);
        var existing = new Supplier { TaxId = "101000007", CommercialName = "Original" };
        db.Suppliers.Add(existing);
        await db.SaveChangesAsync();
        Assert.IsType<BadRequestObjectResult>(await controller.Update(existing.Id,
            new Supplier { TaxId = "00100000000", CommercialName = "No guardar" }));
        db.ChangeTracker.Clear();
        var same = await db.Suppliers.SingleAsync();
        Assert.Equal("Original", same.CommercialName);
        Assert.Equal("101000007", same.TaxId);
    }
}
