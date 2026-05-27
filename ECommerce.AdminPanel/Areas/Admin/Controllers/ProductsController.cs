using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.AdminPanel.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly AdminProductPresenter _presenter;

    public ProductsController(AdminProductPresenter presenter) => _presenter = presenter;

    public async Task<IActionResult> Index()
    {
        var model = await _presenter.GetAllProductsAsync();
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _presenter.GetAllProductsAsync();
        ViewBag.Categories = model.Categories;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductViewModel product)
    {
        if (!ModelState.IsValid) return View(product);
        await _presenter.CreateProductAsync(product);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var categories = await _presenter.GetAllProductsAsync();
        ViewBag.Categories = categories.Categories;
        var product = await _presenter.GetProductByIdAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductViewModel product)
    {
        if (!ModelState.IsValid) return View(product);
        await _presenter.UpdateProductAsync(product);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _presenter.DeleteProductAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
