using ECommerce.Model.Data;
using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_web_project.Controllers;

public class HomeController : Controller
{
    private readonly ProductListPresenter _presenter;
    private readonly ReviewPresenter _reviewPresenter;
    private readonly IRepository<Product> _productRepo;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _context;

    public HomeController(ProductListPresenter presenter, ReviewPresenter reviewPresenter, IRepository<Product> productRepo, UserManager<IdentityUser> userManager, AppDbContext context)
    {
        _presenter = presenter;
        _reviewPresenter = reviewPresenter;
        _productRepo = productRepo;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index(Guid? categoryId, string? search)
    {
        var model = await _presenter.GetProductListAsync(search, categoryId);

        var allProducts = await _context.Products.Include(p => p.Category).ToListAsync();
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

        ViewBag.RecentReviews = await _reviewPresenter.GetRecentReviewsAsync(6);

        return View(model);
    }

    public async Task<IActionResult> Shop(Guid? categoryId, string? search, string? sortBy, int page = 1)
    {
        var model = await _presenter.GetProductListAsync(search, categoryId, sortBy, page, 10);
        return View("Shop", model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _presenter.GetProductByIdAsync(id);
        if (product is null) return NotFound();

        var reviews = await _reviewPresenter.GetReviewsAsync(id);
        product.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        product.ReviewCount = reviews.Count;

        ViewBag.Reviews = reviews;
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(ReviewRequest request)
    {
        var product = await _productRepo.GetByIdAsync(request.ProductId);
        if (product is null) return NotFound();

        string userName;
        string? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            userId = user?.Id;
            userName = user?.Email ?? "Anonymous";
        }
        else
        {
            userName = "Guest";
        }

        await _reviewPresenter.AddReviewAsync(request, userId, userName);
        TempData["ReviewSuccess"] = "Review submitted!";
        return RedirectToAction("Details", new { id = request.ProductId });
    }

    public IActionResult Privacy() => View();

    public IActionResult Faq() => View();

    public IActionResult Contact() => View();
}
