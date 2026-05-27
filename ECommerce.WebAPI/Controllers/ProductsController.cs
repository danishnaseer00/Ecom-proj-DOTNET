using ECommerce.Presenter.Presenters;
using ECommerce.Presenter.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductListPresenter _presenter;

    public ProductsController(ProductListPresenter presenter) => _presenter = presenter;

    [HttpGet]
    public async Task<ActionResult<ProductListViewModel>> GetProducts(
        [FromQuery] Guid? categoryId, [FromQuery] string? search)
    {
        var result = await _presenter.GetProductListAsync(search, categoryId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductViewModel>> GetProduct(Guid id)
    {
        var product = await _presenter.GetProductByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }
}
