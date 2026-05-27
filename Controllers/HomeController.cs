using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_web_project.Controllers;

public class HomeController : Controller
{
    private readonly ProductListPresenter _presenter;
    private readonly IRepository<Product> _productRepo;

    public HomeController(ProductListPresenter presenter, IRepository<Product> productRepo)
    {
        _presenter = presenter;
        _productRepo = productRepo;
    }

    public async Task<IActionResult> Index(Guid? categoryId, string? search)
    {
        var model = await _presenter.GetProductListAsync(search, categoryId);

        var allProducts = await _productRepo.GetAllAsync();
        ViewBag.NewArrivals = allProducts
            .OrderByDescending(p => p.CreatedAt)
            .Take(4)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId
            }).ToList();

        ViewBag.BestSellers = allProducts
            .OrderBy(p => Guid.NewGuid())
            .Take(4)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId
            }).ToList();

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _presenter.GetProductByIdAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    public IActionResult Privacy() => View();

    public IActionResult Faq() => View();

    public IActionResult Contact() => View();
}
