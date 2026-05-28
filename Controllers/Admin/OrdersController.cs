using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_web_project.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly AdminOrderPresenter _presenter;

    public OrdersController(AdminOrderPresenter presenter) => _presenter = presenter;

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
        return RedirectToAction(nameof(Details), new { id });
    }
}
