using ECommerce.Model.Repositories;
using ECommerce.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_web_project.Controllers;

public class ImageController : Controller
{
    private readonly IRepository<Product> _repo;

    public ImageController(IRepository<Product> repo) => _repo = repo;

    public async Task<IActionResult> Product(Guid id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product?.ImageData == null || product.ImageContentType == null)
            return NotFound();
        return File(product.ImageData, product.ImageContentType);
    }
}
