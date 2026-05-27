using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.AdminPanel.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AdminDashboardPresenter _presenter;

    public DashboardController(AdminDashboardPresenter presenter) => _presenter = presenter;

    public async Task<IActionResult> Index()
    {
        var dashboard = await _presenter.GetDashboardAsync();
        return View(dashboard);
    }
}
