using CompraNexus.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Data;

public sealed class CompraNexusDbContext(DbContextOptions<CompraNexusDbContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<UnitMeasure> UnitMeasures => Set<UnitMeasure>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<AccountingEntry> AccountingEntries => Set<AccountingEntry>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Department>().HasIndex(x => x.Name).IsUnique();
        model.Entity<UnitMeasure>().HasIndex(x => x.Description).IsUnique();
        model.Entity<Supplier>().HasIndex(x => x.TaxId).IsUnique();
        model.Entity<PurchaseOrder>().HasIndex(x => x.Number).IsUnique();
        model.Entity<AccountingEntry>().HasIndex(x => x.JournalId).IsUnique();
        model.Entity<Employee>().HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Article>().HasOne<UnitMeasure>().WithMany().HasForeignKey(x => x.UnitMeasureId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<PurchaseOrder>().HasOne(x => x.Department).WithMany().OnDelete(DeleteBehavior.Restrict);
        model.Entity<PurchaseOrder>().HasOne(x => x.Employee).WithMany().OnDelete(DeleteBehavior.Restrict);
        model.Entity<PurchaseOrder>().HasOne(x => x.Supplier).WithMany().OnDelete(DeleteBehavior.Restrict);
        model.Entity<PurchaseOrderLine>().HasOne(x => x.Article).WithMany().OnDelete(DeleteBehavior.Restrict);
        model.Entity<PurchaseOrderLine>().HasOne(x => x.UnitMeasure).WithMany().OnDelete(DeleteBehavior.Restrict);
        model.Entity<PurchaseOrder>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<PurchaseOrder>().HasMany(x => x.AccountingEntries).WithOne(x => x.PurchaseOrder).HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
