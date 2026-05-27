using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.AdminPanel.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IRepository<Category> _repo;

    public CategoriesController(IRepository<Category> repo) => _repo = repo;

    public async Task<IActionResult> Index()
    {
        var categories = await _repo.GetAllAsync();
        var model = categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl
        }).ToList();
        return View(model);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var category = new Category
        {
            Name = model.Name,
            Description = model.Description,
            ImageUrl = model.ImageUrl
        };
        await _repo.AddAsync(category);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null) return NotFound();
        return View(new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var category = await _repo.GetByIdAsync(model.Id);
        if (category == null) return NotFound();
        category.Name = model.Name;
        category.Description = model.Description;
        category.ImageUrl = model.ImageUrl;
        category.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(category);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category != null) await _repo.DeleteAsync(category);
        return RedirectToAction(nameof(Index));
    }
}
