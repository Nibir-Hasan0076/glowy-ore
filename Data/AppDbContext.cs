using GlowyOre.Models;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryImage> CategoryImages => Set<CategoryImage>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug).IsUnique();
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Slug).IsUnique();

        // SQLite stores decimal as TEXT; convert to double so filtering/sorting works.
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion<double>();
        modelBuilder.Entity<Product>().Property(p => p.CompareAtPrice).HasConversion<double?>();
        modelBuilder.Entity<Order>().Property(o => o.Subtotal).HasConversion<double>();
        modelBuilder.Entity<Order>().Property(o => o.DeliveryFee).HasConversion<double>();
        modelBuilder.Entity<Order>().Property(o => o.Total).HasConversion<double>();
        modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasConversion<double>();

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product).WithMany(p => p.OrderItems).HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Images).WithOne(i => i.Category).HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
