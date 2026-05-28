using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Presenters;

public class AdminProductPresenter
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Category> _categoryRepo;

    public AdminProductPresenter(IRepository<Product> productRepo, IRepository<Category> categoryRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<ProductListViewModel> GetAllProductsAsync()
    {
        var products = await _productRepo.GetAllAsync();
        var categories = await _categoryRepo.GetAllAsync();

        return new ProductListViewModel
        {
            Products = products.Select(p => new ProductViewModel
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
            }).ToList()
        };
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepo.GetByIdAsync(id);
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

    public async Task<ProductViewModel> CreateProductAsync(ProductViewModel model, byte[]? imageData = null, string? contentType = null)
    {
        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            ImageUrl = imageData != null ? $"/image/product/{model.Id}" : model.ImageUrl,
            ImageData = imageData,
            ImageContentType = contentType,
            StockQuantity = model.StockQuantity,
            CategoryId = model.CategoryId
        };
        await _productRepo.AddAsync(product);
        model.Id = product.Id;
        if (imageData != null)
        {
            product.ImageUrl = $"/image/product/{product.Id}";
            await _productRepo.UpdateAsync(product);
        }
        model.ImageUrl = product.ImageUrl;
        return model;
    }

    public async Task UpdateProductAsync(ProductViewModel model, byte[]? imageData = null, string? contentType = null)
    {
        var product = await _productRepo.GetByIdAsync(model.Id);
        if (product == null) return;

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.CategoryId = model.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        if (imageData != null)
        {
            product.ImageData = imageData;
            product.ImageContentType = contentType;
            product.ImageUrl = $"/image/product/{product.Id}";
        }

        await _productRepo.UpdateAsync(product);
    }

    public async Task<ProductViewModel?> GetProductByIdWithDataAsync(Guid id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product == null) return null;

        return new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageData != null ? $"/image/product/{product.Id}" : product.ImageUrl,
            StockQuantity = product.StockQuantity,
            CategoryName = product.Category?.Name ?? "",
            CategoryId = product.CategoryId
        };
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product != null)
            await _productRepo.DeleteAsync(product);
    }
}
