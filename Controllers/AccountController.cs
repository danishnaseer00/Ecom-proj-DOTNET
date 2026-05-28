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
    private readonly IEmailSender _emailSender;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ICustomerRepository customerRepo,
        IRepository<Cart> cartRepo,
        IRepository<CartItem> cartItemRepo,
        OrderPresenter orderPresenter,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _customerRepo = customerRepo;
        _cartRepo = cartRepo;
        _cartItemRepo = cartItemRepo;
        _orderPresenter = orderPresenter;
        _emailSender = emailSender;
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

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmLink = Url.Action("ConfirmEmail", "Account",
            new { userId = user.Id, token }, Request.Scheme)!;

        var body = $"""
            <h2>Welcome to Store!</h2>
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href="{confirmLink}">Confirm Email</a></p>
            <p>If you didn't create an account, you can ignore this email.</p>
            """;

        try
        {
            await _emailSender.SendEmailAsync(email, "Confirm your email", body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email] Failed to send confirmation to {email}: {ex.Message}");
        }

        TempData["ConfirmEmailLink"] = confirmLink;
        TempData["ConfirmEmail"] = email;
        return RedirectToAction("RegisterConfirmation");
    }

    [HttpGet]
    public IActionResult RegisterConfirmation() => View();

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Index", "Home");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return View("Error");

        var customer = await _customerRepo.GetByUserIdAsync(user.Id);
        if (customer != null) await MergeSessionCartAsync(customer);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return View("ConfirmEmail");
    }

    [HttpGet]
    public IActionResult Login(string? email) 
    {
        if (!string.IsNullOrEmpty(email))
            ViewBag.UnconfirmedEmail = email;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError("", "You must confirm your email before logging in.");
            ViewBag.UnconfirmedEmail = email;
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        var customer = await _customerRepo.GetByUserIdAsync(user.Id);
        if (customer != null) await MergeSessionCartAsync(customer);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ResendConfirmation() => View();

    [HttpPost]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Email is required.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("", "No account found with that email.");
            return View();
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError("", "This email is already confirmed. Please log in.");
            return View();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmLink = Url.Action("ConfirmEmail", "Account",
            new { userId = user.Id, token }, Request.Scheme)!;

        var body = $"""
            <h2>Welcome to Store!</h2>
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href="{confirmLink}">Confirm Email</a></p>
            <p>If you didn't create an account, you can ignore this email.</p>
            """;

        try
        {
            await _emailSender.SendEmailAsync(email, "Confirm your email", body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email] Failed to send confirmation to {email}: {ex.Message}");
        }

        TempData["ConfirmEmailLink"] = confirmLink;
        TempData["ConfirmEmail"] = email;
        return RedirectToAction("RegisterConfirmation");
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

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return View(new Customer { UserId = user.Id, FirstName = "Admin", LastName = "User" });
        }

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

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Admin profiles cannot be edited.";
            return RedirectToAction("Profile");
        }

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
