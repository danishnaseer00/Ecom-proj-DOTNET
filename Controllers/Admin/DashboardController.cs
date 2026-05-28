using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_web_project.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AdminDashboardPresenter _presenter;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICustomerRepository _customerRepo;

    public DashboardController(
        AdminDashboardPresenter presenter,
        UserManager<IdentityUser> userManager,
        ICustomerRepository customerRepo)
    {
        _presenter = presenter;
        _userManager = userManager;
        _customerRepo = customerRepo;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _presenter.GetDashboardAsync();
        var adminUserIds = (await _userManager.GetUsersInRoleAsync("Admin"))
            .Select(u => u.Id)
            .ToHashSet();
        var allCustomers = await _customerRepo.GetAllAsync();
        dashboard.TotalCustomers = allCustomers.Count(c => !adminUserIds.Contains(c.UserId));
        return View(dashboard);
    }
}
