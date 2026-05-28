using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Ecommerce_web_project.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly AdminOrderPresenter _presenter;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICustomerRepository _customerRepo;

    public OrdersController(AdminOrderPresenter presenter, IEmailSender emailSender, UserManager<IdentityUser> userManager, ICustomerRepository customerRepo)
    {
        _presenter = presenter;
        _emailSender = emailSender;
        _userManager = userManager;
        _customerRepo = customerRepo;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _presenter.GetAllOrdersAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _presenter.GetOrderByIdAsync(id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        await _presenter.UpdateOrderStatusAsync(id, status);

        if (string.Equals(status, "Shipped", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var orderEntity = await _presenter.GetOrderEntityAsync(id);
                if (orderEntity != null)
                {
                    var customer = await _customerRepo.GetByIdAsync(orderEntity.CustomerId);
                    if (customer != null)
                    {
                        var user = await _userManager.FindByIdAsync(customer.UserId);
                        if (user?.Email != null)
                        {
                            var itemsHtml = new StringBuilder();
                            foreach (var item in orderEntity.Items)
                            {
                                var name = item.Product?.Name ?? "Unknown";
                                    var subtotal = item.Quantity * item.UnitPrice;
                        itemsHtml.Append($"<tr><td style=\"padding:8px;border-bottom:1px solid #dee2e6;\">{name}</td><td style=\"padding:8px;border-bottom:1px solid #dee2e6;text-align:center;\">{item.Quantity}</td><td style=\"padding:8px;border-bottom:1px solid #dee2e6;text-align:right;\">${subtotal:F2}</td></tr>");
                            }

                            var body = $"""
                                <div style="font-family:'Inter',sans-serif;max-width:560px;margin:0 auto;">
                                    <div style="background:#2a2a3d;color:#fff;padding:1.5rem 2rem;text-align:center;border-radius:8px 8px 0 0;">
                                        <h2 style="margin:0;font-size:1.25rem;">🚚 Your Order is on its Way!</h2>
                                    </div>
                                    <div style="padding:2rem;background:#f8f9fa;border:1px solid #dee2e6;border-top:none;border-radius:0 0 8px 8px;">
                                        <p style="margin:0 0 1rem;color:#2a2a3d;">Hi <strong>{customer.FirstName}</strong>,</p>
                                        <p style="margin:0 0 1rem;color:#495057;">Your order <strong>#{orderEntity.OrderNumber}</strong> has been shipped and is on its way!</p>
                                        <table style="width:100%;border-collapse:collapse;margin-bottom:1rem;font-size:0.875rem;">
                                            <thead><tr style="background:#2a2a3d;color:#fff;"><th style="padding:8px;text-align:left;">Item</th><th style="padding:8px;text-align:center;">Qty</th><th style="padding:8px;text-align:right;">Total</th></tr></thead>
                                            <tbody>{itemsHtml}</tbody>
                                        </table>
                                        <p style="text-align:right;font-size:1.1rem;font-weight:700;color:#2a2a3d;margin:0 0 1rem;">Total: ${orderEntity.TotalAmount:F2}</p>
                                        <p style="margin:0 0 0.5rem;color:#495057;">Thank you for shopping with DanStore!</p>
                                        <p style="margin:0;color:#6c757d;font-size:0.8rem;">— DanStore Team</p>
                                    </div>
                                </div>
                                """;

                            await _emailSender.SendEmailAsync(user.Email, $"Your Order #{orderEntity.OrderNumber} Has Been Shipped!", body);
                            TempData["Success"] = $"Shipped email sent to {user.Email}";
                        }
                    }
                }
            }
            catch { }
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
