using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.AdminPanel.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CustomersController : Controller
{
    private readonly AdminDashboardPresenter _presenter;

    public CustomersController(AdminDashboardPresenter presenter) => _presenter = presenter;

    public async Task<IActionResult> Index()
    {
        var customers = await _presenter.GetAllCustomersAsync();
        return View(customers);
    }
}
