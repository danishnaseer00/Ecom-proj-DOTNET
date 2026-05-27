using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderPresenter _presenter;

    public OrdersController(OrderPresenter presenter) => _presenter = presenter;

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var customerId = GetCustomerId();
        var result = await _presenter.GetOrdersAsync(customerId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _presenter.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    private Guid GetCustomerId()
    {
        var claim = User.FindFirst("customerId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
