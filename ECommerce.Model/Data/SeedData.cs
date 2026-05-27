using ECommerce.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Model.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var electronics = new Category { Name = "Electronics", Description = "Electronic devices and accessories" };
        var clothing = new Category { Name = "Clothing", Description = "Apparel and fashion" };
        var home = new Category { Name = "Home & Garden", Description = "Home improvement and garden supplies" };
        var books = new Category { Name = "Books", Description = "Books and media" };

        context.Categories.AddRange(electronics, clothing, home, books);
        await context.SaveChangesAsync();

        context.Products.AddRange(
            new Product { Name = "Wireless Headphones", Price = 79.99m, StockQuantity = 50, CategoryId = electronics.Id, ImageUrl = "https://images.unsplash.com/photo-1520170350707-b2da59970118?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "Bluetooth Speaker", Price = 49.99m, StockQuantity = 30, CategoryId = electronics.Id, ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "Cotton T-Shirt", Price = 19.99m, StockQuantity = 100, CategoryId = clothing.Id, ImageUrl = "https://images.unsplash.com/photo-1581655353564-df123a1eb820?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "Denim Jacket", Price = 89.99m, StockQuantity = 25, CategoryId = clothing.Id, ImageUrl = "https://images.unsplash.com/photo-1537465978529-d23b17165b3b?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "Indoor Plant Pot", Price = 34.99m, StockQuantity = 40, CategoryId = home.Id, ImageUrl = "https://images.unsplash.com/photo-1592150621744-aca64f48394a?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "Cooking Pan Set", Price = 129.99m, StockQuantity = 15, CategoryId = home.Id, ImageUrl = "https://images.unsplash.com/photo-1541508168132-0b1d81249c25?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "C# Programming Guide", Price = 39.99m, StockQuantity = 60, CategoryId = books.Id, ImageUrl = "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?auto=format&fit=crop&w=600&h=600&q=80" },
            new Product { Name = "Design Patterns Book", Price = 44.99m, StockQuantity = 45, CategoryId = books.Id, ImageUrl = "https://images.unsplash.com/photo-1529590003495-b2646e2718bf?auto=format&fit=crop&w=600&h=600&q=80" }
        );
        await context.SaveChangesAsync();
    }
}
