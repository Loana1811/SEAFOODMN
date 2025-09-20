using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ASM_GROUP3.Models;

public partial class SeafoodManagementContext : DbContext
{
    public SeafoodManagementContext()
    {
    }

    public SeafoodManagementContext(DbContextOptions<SeafoodManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ImportOrder> ImportOrders { get; set; }

    public virtual DbSet<ImportOrderDetail> ImportOrderDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Seafood> Seafoods { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=ADMIN\\SQLEXPRESS;Database=SeafoodManagement;User Id=sa;Password=18112004;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__ImportOr__C3905BAF993E2C36");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ImportOrders)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__ImportOrd__Suppl__3B75D760");
        });

        modelBuilder.Entity<ImportOrderDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__ImportOr__135C314D40405A61");

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.SeafoodId).HasColumnName("SeafoodID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.ImportOrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__ImportOrd__Order__3E52440B");

            entity.HasOne(d => d.Seafood).WithMany(p => p.ImportOrderDetails)
                .HasForeignKey(d => d.SeafoodId)
                .HasConstraintName("FK__ImportOrd__Seafo__3F466844");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF63379009");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Customer).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C58155D0F");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.SeafoodId).HasColumnName("SeafoodID");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", true)
                .HasColumnType("decimal(29, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__Order__440B1D61");

            entity.HasOne(d => d.Seafood).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.SeafoodId)
                .HasConstraintName("FK__OrderDeta__Seafo__44FF419A");
        });

        modelBuilder.Entity<Seafood>(entity =>
        {
            entity.HasKey(e => e.SeafoodId).HasName("PK__Seafoods__6EB2F8C2D6153EB7");

            entity.Property(e => e.SeafoodId).HasColumnName("SeafoodID");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.ImportDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666940164B3F9");

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACA267EE53");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(10);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
