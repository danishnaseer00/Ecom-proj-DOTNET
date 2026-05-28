using ECommerce.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Model.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var electronics = new Category { Name = "Electronics", Description = "Electronic devices and accessories" };
            var clothing = new Category { Name = "Clothing", Description = "Apparel and fashion" };
            var home = new Category { Name = "Home & Garden", Description = "Home improvement and garden supplies" };
            var books = new Category { Name = "Books", Description = "Books and media" };

            context.Categories.AddRange(electronics, clothing, home, books);
            await context.SaveChangesAsync();

            context.Products.AddRange(
                new Product { Name = "Wireless Headphones", Price = 79.99m, StockQuantity = 50, CategoryId = electronics.Id, ImageUrl = "/images/products/wireless%20headphones.webp" },
                new Product { Name = "Bluetooth Speaker", Price = 49.99m, StockQuantity = 30, CategoryId = electronics.Id, ImageUrl = "/images/products/bluetooth.jfif" },
                new Product { Name = "Cotton T-Shirt", Price = 19.99m, StockQuantity = 100, CategoryId = clothing.Id, ImageUrl = "/images/products/cotton-t-shirt.jfif" },
                new Product { Name = "Denim Jacket", Price = 89.99m, StockQuantity = 25, CategoryId = clothing.Id, ImageUrl = "/images/products/jacket.webp" },
                new Product { Name = "Indoor Plant Pot", Price = 34.99m, StockQuantity = 40, CategoryId = home.Id, ImageUrl = "/images/products/plant.avif" },
                new Product { Name = "Cooking Pan Set", Price = 129.99m, StockQuantity = 15, CategoryId = home.Id, ImageUrl = "/images/products/pan.webp" },
                new Product { Name = "C# Programming Guide", Price = 39.99m, StockQuantity = 60, CategoryId = books.Id, ImageUrl = "/images/products/c-lang-book.jfif" },
                new Product { Name = "Design Patterns Book", Price = 44.99m, StockQuantity = 45, CategoryId = books.Id, ImageUrl = "/images/products/Design-Patterns-Elements-of-Reusable-Object-Oriented-Software-by-Erich-Gamma.webp" }
            );
            await context.SaveChangesAsync();
        }

        await UpdateProductImagesAsync(context);
    }

    private static async Task UpdateProductImagesAsync(AppDbContext context)
    {
        var localImages = new Dictionary<string, string>
        {
            ["Wireless Headphones"] = "/images/products/wireless%20headphones.webp",
            ["Bluetooth Speaker"] = "/images/products/bluetooth.jfif",
            ["Cotton T-Shirt"] = "/images/products/cotton-t-shirt.jfif",
            ["Denim Jacket"] = "/images/products/jacket.webp",
            ["Indoor Plant Pot"] = "/images/products/plant.avif",
            ["Cooking Pan Set"] = "/images/products/pan.webp",
            ["C# Programming Guide"] = "/images/products/c-lang-book.jfif",
            ["Design Patterns Book"] = "/images/products/Design-Patterns-Elements-of-Reusable-Object-Oriented-Software-by-Erich-Gamma.webp"
        };

        var products = await context.Products.ToListAsync();
        var changed = false;
        foreach (var product in products)
        {
            if (localImages.TryGetValue(product.Name, out var localPath) && product.ImageUrl != localPath)
            {
                product.ImageUrl = localPath;
                changed = true;
            }
        }
        if (changed) await context.SaveChangesAsync();
    }
}
