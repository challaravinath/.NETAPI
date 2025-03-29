// Data/DbInitializer.cs
using ProductApi.Models;

namespace ProductApi.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context)
    {
        // Check if DB has been seeded
        if (context.Products.Any())
        {
            return; // DB has been seeded
        }

        // Seed with sample data
        var products = new List<Product>
        {
            new Product { Name = "Laptop", Price = 999.99m, StockQuantity = 50 },
            new Product { Name = "Smartphone", Price = 699.99m, StockQuantity = 100 },
            new Product { Name = "Headphones", Price = 149.99m, StockQuantity = 200 },
            new Product { Name = "Tablet", Price = 399.99m, StockQuantity = 75 },
            new Product { Name = "Monitor", Price = 249.99m, StockQuantity = 30 }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}