using System.ComponentModel.DataAnnotations;

namespace CompraNexus.Api.Contracts;

public sealed class PurchaseOrderInput
{
    [Required, StringLength(40)] public string Number { get; set; } = "";
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required, RegularExpression("Borrador|Confirmada|Anulada")] public string Status { get; set; } = "Borrador";
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Range(1, int.MaxValue)] public int EmployeeId { get; set; }
    [Range(1, int.MaxValue)] public int SupplierId { get; set; }
    [MinLength(1)] public List<PurchaseOrderLineInput> Lines { get; set; } = [];
}

public sealed class PurchaseOrderLineInput
{
    [Range(1, int.MaxValue)] public int ArticleId { get; set; }
    [Range(1, int.MaxValue)] public int Quantity { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999")] public decimal UnitCost { get; set; }
}

public sealed record PurchaseOrderLineView(int Id, int ArticleId, string Article, int UnitMeasureId,
    string UnitMeasure, int Quantity, decimal UnitCost);
public sealed record PurchaseOrderView(int Id, string Number, DateOnly OrderDate, string Status,
    int DepartmentId, string Department, int EmployeeId, string Employee,
    int SupplierId, string Supplier, decimal Total, List<PurchaseOrderLineView> Lines);

public sealed record DashboardCounts(int Departments, int Articles, int Suppliers, int Orders);
