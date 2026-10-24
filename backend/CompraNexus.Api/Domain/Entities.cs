using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompraNexus.Api.Domain;

public interface ICatalogEntity
{
    int Id { get; set; }
    string Status { get; set; }
}

public class Department : ICatalogEntity
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, RegularExpression("Activo|Inactivo")] public string Status { get; set; } = "Activo";
}

public class UnitMeasure : ICatalogEntity
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Description { get; set; } = "";
    [Required, RegularExpression("Activo|Inactivo")] public string Status { get; set; } = "Activo";
}

public class Supplier : ICatalogEntity
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string TaxId { get; set; } = "";
    [Required, StringLength(150)] public string CommercialName { get; set; } = "";
    [Required, RegularExpression("Activo|Inactivo")] public string Status { get; set; } = "Activo";
}

public class Employee : ICatalogEntity
{
    public int Id { get; set; }
    [Required, StringLength(140)] public string Name { get; set; } = "";
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Required, RegularExpression("Activo|Inactivo")] public string Status { get; set; } = "Activo";
}

public class Article : ICatalogEntity
{
    public int Id { get; set; }
    [Required, StringLength(180)] public string Description { get; set; } = "";
    [Required, StringLength(100)] public string Brand { get; set; } = "";
    [Range(1, int.MaxValue)] public int UnitMeasureId { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    [Required, RegularExpression("Activo|Inactivo")] public string Status { get; set; } = "Activo";
}

public class PurchaseOrder
{
    public int Id { get; set; }
    [Required, StringLength(40)] public string Number { get; set; } = "";
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required, RegularExpression("Borrador|Confirmada|Anulada")] public string Status { get; set; } = "Borrador";
    public int DepartmentId { get; set; }
    public int EmployeeId { get; set; }
    public int SupplierId { get; set; }
    public Department Department { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public List<PurchaseOrderLine> Lines { get; set; } = [];
    public List<AccountingEntry> AccountingEntries { get; set; } = [];
}

public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    public int UnitMeasureId { get; set; }
    public UnitMeasure UnitMeasure { get; set; } = null!;
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitCost { get; set; }
}

public class AccountingEntry
{
    public int Id { get; set; }
    public Guid JournalId { get; set; } = Guid.NewGuid();
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    [MaxLength(255)] public string Description { get; set; } = "";
    [MaxLength(40)] public string InventoryTypeId { get; set; } = "";
    [MaxLength(40)] public string Account { get; set; } = "";
    [MaxLength(2)] public string Movement { get; set; } = "";
    public DateOnly EntryDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Pendiente";
    public int Attempts { get; set; }
    [MaxLength(2000)] public string? Response { get; set; }
}
