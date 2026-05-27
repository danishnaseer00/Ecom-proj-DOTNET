using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
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

    public CheckoutController(
        CheckoutPresenter checkoutPresenter,
        CartPresenter cartPresenter,
        OrderPresenter orderPresenter,
        ICustomerRepository customerRepo,
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IConfiguration config)
    {
        _checkoutPresenter = checkoutPresenter;
        _cartPresenter = cartPresenter;
        _orderPresenter = orderPresenter;
        _customerRepo = customerRepo;
        _userManager = userManager;
        _emailSender = emailSender;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sessionItems = GetCart();
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
    public async Task<IActionResult> PlaceOrder(string street, string city, string state, string zipCode, string country, string paymentIntentId)
    {
        var sessionItems = GetCart();
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

    private List<CartItemSession> GetCart() =>
        JsonSerializer.Deserialize<List<CartItemSession>>(HttpContext.Session.GetString("Cart") ?? "[]") ?? new();
}
