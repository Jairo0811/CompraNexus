using CompraNexus.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CompraNexus.Api.Data;

public sealed class CompraNexusDbContext(DbContextOptions<CompraNexusDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
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
        model.Entity<AppUser>().ToTable("AppUsers").HasIndex(x => x.NormalizedEmail).IsUnique();
        model.Entity<AppUser>().Property(x => x.Email).HasMaxLength(180).IsRequired();
        model.Entity<AppUser>().Property(x => x.NormalizedEmail).HasMaxLength(180).IsRequired();
        model.Entity<AppUser>().Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        model.Entity<AppUser>().Property(x => x.Role).HasMaxLength(40).IsRequired();
        model.Entity<AppUser>().Property(x => x.SecurityStamp).HasMaxLength(64).IsRequired();
        model.Entity<Department>().HasIndex(x => x.Name).IsUnique();
        model.Entity<UnitMeasure>().HasIndex(x => x.Description).IsUnique();
        model.Entity<Supplier>().HasIndex(x => x.TaxId).IsUnique();
        model.Entity<PurchaseOrder>().HasIndex(x => x.Number).IsUnique();
        model.Entity<AccountingEntry>().HasIndex(x => x.JournalId).IsUnique();
        // Un solo DB y un solo CR por orden: evita duplicados ante solicitudes concurrentes.
        model.Entity<AccountingEntry>().HasIndex(x => new { x.PurchaseOrderId, x.Movement }).IsUnique();
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
