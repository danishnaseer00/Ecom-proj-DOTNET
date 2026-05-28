using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_web_project.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly AdminProductPresenter _presenter;

    public ProductsController(AdminProductPresenter presenter)
    {
        _presenter = presenter;
    }

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
    public async Task<IActionResult> Create(ProductViewModel product, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _presenter.GetAllProductsAsync();
            ViewBag.Categories = categories.Categories;
            return View(product);
        }
        var (data, contentType) = await ReadFileAsync(imageFile);
        await _presenter.CreateProductAsync(product, data, contentType);
        TempData["Success"] = data != null
            ? "Product created with image successfully."
            : "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var categories = await _presenter.GetAllProductsAsync();
        ViewBag.Categories = categories.Categories;
        var product = await _presenter.GetProductByIdWithDataAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductViewModel product, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _presenter.GetAllProductsAsync();
            ViewBag.Categories = categories.Categories;
            return View(product);
        }
        var (data, contentType) = await ReadFileAsync(imageFile);
        await _presenter.UpdateProductAsync(product, data, contentType);
        TempData["Success"] = data != null
            ? "Product updated with new image."
            : "Product updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _presenter.DeleteProductAsync(id);
        TempData["Success"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static async Task<(byte[]? data, string? contentType)> ReadFileAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return (null, null);
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return (ms.ToArray(), file.ContentType);
    }
}
