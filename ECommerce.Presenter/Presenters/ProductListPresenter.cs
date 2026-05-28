using ECommerce.Model.Data;
using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ProductListViewModel> GetProductListAsync(string? searchTerm = null, Guid? categoryId = null)
    {
        var products = await _productRepo.GetAllAsync();
        var categories = await _categoryRepo.GetAllAsync();

        var query = products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return new ProductListViewModel
        {
            Products = query.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageData != null ? $"/image/product/{p.Id}" : p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId
            }).ToList(),
            Categories = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl
            }).ToList(),
            SearchTerm = searchTerm,
            CategoryId = categoryId
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
