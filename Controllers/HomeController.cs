using ECommerce.Model.Data;
using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _context;

    public HomeController(ProductListPresenter presenter, ReviewPresenter reviewPresenter, IRepository<Product> productRepo, UserManager<IdentityUser> userManager, IEmailSender emailSender, AppDbContext context)
    {
        _presenter = presenter;
        _reviewPresenter = reviewPresenter;
        _productRepo = productRepo;
        _userManager = userManager;
        _emailSender = emailSender;
        _context = context;
    }

    public async Task<IActionResult> Index(Guid? categoryId, string? search)
    {
        var model = await _presenter.GetProductListAsync(search, categoryId);

        var allProducts = await _context.Products.Include(p => p.Category).ToListAsync();
        ViewBag.NewArrivals = allProducts
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
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

        var salesData = await _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
            .ToListAsync();

        var bestSellerIds = salesData
            .OrderByDescending(s => s.TotalSold)
            .Take(5)
            .Select(s => s.ProductId)
            .ToHashSet();

        ViewBag.BestSellers = allProducts
            .Where(p => bestSellerIds.Contains(p.Id))
            .OrderByDescending(p => bestSellerIds.Contains(p.Id) ? salesData.First(s => s.ProductId == p.Id).TotalSold : 0)
            .Take(5)
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

        if (ViewBag.BestSellers.Count == 0)
        {
            ViewBag.BestSellers = allProducts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
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
        }

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
    [Authorize]
    public async Task<IActionResult> AddReview(ReviewRequest request)
    {
        var product = await _productRepo.GetByIdAsync(request.ProductId);
        if (product is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        await _reviewPresenter.AddReviewAsync(request, user!.Id, user.Email ?? "Anonymous");
        TempData["ReviewSuccess"] = "Review submitted!";
        return RedirectToAction("Details", new { id = request.ProductId });
    }

    public IActionResult Privacy() => View();

    public IActionResult Faq() => View();

    [HttpGet]
    public IActionResult Contact() => View();

    [HttpPost]
    public async Task<IActionResult> Contact(string name, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError("", "All fields are required.");
            return View();
        }

        var body = $"""
            <h2>New Contact Message</h2>
            <p><strong>Name:</strong> {name}</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Message:</strong></p>
            <p>{message}</p>
            """;

        try
        {
            await _emailSender.SendEmailAsync("danishnaseer000@gmail.com", $"Contact Form — {name}", body);
            TempData["Success"] = "Message sent! We'll get back to you soon.";
        }
        catch
        {
            ModelState.AddModelError("", "Failed to send message. Please try again.");
            return View();
        }

        return RedirectToAction("Contact");
    }
}
