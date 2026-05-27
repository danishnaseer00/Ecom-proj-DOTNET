using ECommerce.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Model.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(e =>
        {
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
            e.Property(p => p.Price).HasPrecision(18, 2);
        });

        builder.Entity<Cart>(e =>
        {
            e.HasOne(c => c.Customer).WithOne(c => c.Cart).HasForeignKey<Cart>(c => c.CustomerId);
            e.HasMany(c => c.Items).WithOne(i => i.Cart).HasForeignKey(i => i.CartId);
        });

        builder.Entity<CartItem>(e => e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId));

        builder.Entity<Order>(e =>
        {
            e.HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);
            e.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
            e.HasOne(o => o.Payment).WithOne(p => p.Order).HasForeignKey<Payment>(p => p.OrderId);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.Property(o => o.Status).HasConversion<string>();
            e.OwnsOne(o => o.ShippingAddress);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId);
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });

        builder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.Property(p => p.Status).HasConversion<string>();
        });

        builder.Entity<Customer>(e => e.OwnsOne(c => c.DefaultAddress));
    }
}
