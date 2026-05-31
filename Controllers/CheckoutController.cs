using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ecommerce_web_project.Controllers;

public class CheckoutController : Controller
{
    private readonly CheckoutPresenter _checkoutPresenter;
    private readonly CartPresenter _cartPresenter;
    private readonly OrderPresenter _orderPresenter;
    private readonly ICustomerRepository _customerRepo;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly IRepository<Cart> _cartRepo;
    private readonly IRepository<CartItem> _cartItemRepo;

    public CheckoutController(
        CheckoutPresenter checkoutPresenter,
        CartPresenter cartPresenter,
        OrderPresenter orderPresenter,
        ICustomerRepository customerRepo,
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IConfiguration config,
        IRepository<Cart> cartRepo,
        IRepository<CartItem> cartItemRepo)
    {
        _checkoutPresenter = checkoutPresenter;
        _cartPresenter = cartPresenter;
        _orderPresenter = orderPresenter;
        _customerRepo = customerRepo;
        _userManager = userManager;
        _emailSender = emailSender;
        _config = config;
        _cartRepo = cartRepo;
        _cartItemRepo = cartItemRepo;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var sessionItems = await GetCartAsync();
        if (sessionItems.Count == 0)
            return RedirectToAction("Index", "Cart");

        string? customerId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var customer = await _customerRepo.GetByUserIdAsync(user.Id);
                if (customer != null)
                    customerId = customer.Id.ToString();
            }
        }

        var model = await _checkoutPresenter.PrepareCheckoutAsync(customerId, sessionItems);
        ViewBag.StripePublishableKey = _config["Stripe:PublishableKey"];
        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PlaceOrder(string street, string city, string state, string zipCode, string country, string paymentIntentId)
    {
        var sessionItems = await GetCartAsync();
        if (sessionItems.Count == 0)
            return RedirectToAction("Index", "Cart");

        string? customerId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var customer = await _customerRepo.GetByUserIdAsync(user.Id);
                if (customer != null)
                    customerId = customer.Id.ToString();
            }
        }

        var request = new CheckoutRequest
        {
            Street = street,
            City = city,
            State = state,
            ZipCode = zipCode,
            Country = country
        };

        var order = await _checkoutPresenter.PlaceOrderAsync(customerId, sessionItems, request, paymentIntentId);
        if (order == null)
        {
            TempData["Error"] = "Failed to place order.";
            return RedirectToAction("Index");
        }

        HttpContext.Session.Remove("Cart");

        var userEmail = User.Identity?.IsAuthenticated == true
            ? (await _userManager.GetUserAsync(User))?.Email
            : null;
        if (userEmail != null)
        {
            var itemsHtml = string.Join("", order.Items.Select(i =>
                $"<tr><td>{i.ProductName}</td><td>{i.Quantity}</td><td>${i.Subtotal:F2}</td></tr>"));
            var body = $"""
                <h2>Order Confirmed — {order.OrderNumber}</h2>
                <p>Thank you for your purchase!</p>
                <table border="1" cellpadding="8" cellspacing="0" style="border-collapse:collapse;width:100%">
                    <thead><tr><th>Item</th><th>Qty</th><th>Total</th></tr></thead>
                    <tbody>{itemsHtml}</tbody>
                </table>
                <p><strong>Total: ${order.TotalAmount:F2}</strong></p>
                <p>Your order will be processed shortly.</p>
                """;
            try { await _emailSender.SendEmailAsync(userEmail, $"Order Confirmed — {order.OrderNumber}", body); }
            catch { }
        }

        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(Guid id)
    {
        var order = await _orderPresenter.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    private async Task<List<CartItemSession>> GetCartAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var customer = await _customerRepo.GetByUserIdAsync(user.Id);
                if (customer != null)
                {
                    var dbCart = (await _cartRepo.FindAsync(c => c.CustomerId == customer.Id)).FirstOrDefault();
                    if (dbCart != null)
                    {
                        var dbItems = await _cartItemRepo.FindAsync(i => i.CartId == dbCart.Id);
                        return dbItems.Select(i => new CartItemSession { ProductId = i.ProductId, Quantity = i.Quantity }).ToList();
                    }
                }
            }
        }
        return JsonSerializer.Deserialize<List<CartItemSession>>(HttpContext.Session.GetString("Cart") ?? "[]") ?? new();
    }
}
