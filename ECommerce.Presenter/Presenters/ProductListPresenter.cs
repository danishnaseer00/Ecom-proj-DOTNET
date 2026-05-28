using ECommerce.Model.Data;
using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ECommerce.Presenter.Presenters;

public class ProductListPresenter
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly AppDbContext _context;

    public ProductListPresenter(IRepository<Product> productRepo, IRepository<Category> categoryRepo, AppDbContext context)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _context = context;
    }

    public async Task<ProductListViewModel> GetProductListAsync(string? searchTerm = null, Guid? categoryId = null, string? sortBy = null, int page = 1, int pageSize = 20)
    {
        var categories = await _categoryRepo.GetAllAsync();
        var products = await _productRepo.GetAllAsync();

        var query = products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || (p.Category != null && p.Category.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        sortBy = sortBy?.ToLower();
        query = sortBy switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "name-asc" => query.OrderBy(p => p.Name),
            "name-desc" => query.OrderByDescending(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = query.Count();
        var pageProducts = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var productIds = pageProducts.Select(p => p.Id).ToList();
        var allReviews = await _context.Reviews.Where(r => productIds.Contains(r.ProductId)).ToListAsync();
        var ratingLookup = allReviews.GroupBy(r => r.ProductId).ToDictionary(g => g.Key, g => g.Average(r => (double)r.Rating));
        var countLookup = allReviews.GroupBy(r => r.ProductId).ToDictionary(g => g.Key, g => g.Count());

        return new ProductListViewModel
        {
            Products = pageProducts.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageData != null ? $"/image/product/{p.Id}" : p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId,
                AverageRating = ratingLookup.GetValueOrDefault(p.Id, 0),
                ReviewCount = countLookup.GetValueOrDefault(p.Id, 0)
            }).ToList(),
            Categories = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl
            }).ToList(),
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            SortBy = sortBy ?? "newest",
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        return new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageData != null ? $"/image/product/{product.Id}" : product.ImageUrl,
            StockQuantity = product.StockQuantity,
            CategoryName = product.Category.Name,
            CategoryId = product.CategoryId
        };
    }
}
