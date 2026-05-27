using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ecommerce_web_project.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ICustomerRepository _customerRepo;
    private readonly IRepository<Cart> _cartRepo;
    private readonly IRepository<CartItem> _cartItemRepo;
    private readonly OrderPresenter _orderPresenter;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ICustomerRepository customerRepo,
        IRepository<Cart> cartRepo,
        IRepository<CartItem> cartItemRepo,
        OrderPresenter orderPresenter)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _customerRepo = customerRepo;
        _cartRepo = cartRepo;
        _cartItemRepo = cartItemRepo;
        _orderPresenter = orderPresenter;
    }

    private async Task MergeSessionCartAsync(Customer customer)
    {
        var sessionItems = JsonSerializer.Deserialize<List<CartItemSession>>(HttpContext.Session.GetString("Cart") ?? "[]") ?? new();
        if (sessionItems.Count == 0) return;

        var existingCart = (await _cartRepo.FindAsync(c => c.CustomerId == customer.Id)).FirstOrDefault();
        if (existingCart == null)
        {
            existingCart = new Cart { CustomerId = customer.Id };
            await _cartRepo.AddAsync(existingCart);
        }

        var existingItems = await _cartItemRepo.FindAsync(i => i.CartId == existingCart.Id);
        foreach (var si in sessionItems)
        {
            var existingItem = existingItems.FirstOrDefault(i => i.ProductId == si.ProductId);
            if (existingItem != null)
                existingItem.Quantity += si.Quantity;
            else
                await _cartItemRepo.AddAsync(new CartItem { CartId = existingCart.Id, ProductId = si.ProductId, Quantity = si.Quantity });
        }

        HttpContext.Session.Remove("Cart");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string firstName, string lastName, string email, string phone, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return View();
        }

        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            return View();
        }

        var customer = new Customer
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone
        };
        await _customerRepo.AddAsync(customer);

        await _signInManager.SignInAsync(user, isPersistent: false);
        await MergeSessionCartAsync(customer);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var customer = await _customerRepo.GetByUserIdAsync(user.Id);
            if (customer != null) await MergeSessionCartAsync(customer);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var customer = await _customerRepo.GetByUserIdAsync(user.Id);
        if (customer == null)
        {
            customer = new Customer { UserId = user.Id, FirstName = "", LastName = "" };
            await _customerRepo.AddAsync(customer);
        }

        return View(customer);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Profile(Customer model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var customer = await _customerRepo.GetByUserIdAsync(user.Id);
        if (customer == null) return NotFound();

        customer.FirstName = model.FirstName;
        customer.LastName = model.LastName;
        customer.Phone = model.Phone;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customerRepo.UpdateAsync(customer);

        TempData["Success"] = "Profile updated.";
        return RedirectToAction("Profile");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var customer = await _customerRepo.GetByUserIdAsync(user.Id);
        if (customer == null)
        {
            return View(new ECommerce.Presenter.ViewModels.OrderListViewModel { Orders = new() });
        }

        var model = await _orderPresenter.GetOrdersAsync(customer.Id);
        return View(model);
    }
}
