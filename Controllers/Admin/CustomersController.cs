using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_web_project.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CustomersController : Controller
{
    private readonly AdminDashboardPresenter _presenter;
    private readonly UserManager<IdentityUser> _userManager;

    public CustomersController(AdminDashboardPresenter presenter, UserManager<IdentityUser> userManager)
    {
        _presenter = presenter;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _presenter.GetAllCustomersAsync();
        var adminUserIds = (await _userManager.GetUsersInRoleAsync("Admin"))
            .Select(u => u.Id)
            .ToHashSet();
        var filtered = customers.Where(c => !adminUserIds.Contains(c.UserId)).ToList();
        return View(filtered);
    }
}
