using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly CheckoutPresenter _presenter;

    public CheckoutController(CheckoutPresenter presenter) => _presenter = presenter;

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] Presenter.ViewModels.CheckoutRequest request)
    {
        var customerId = GetCustomerId().ToString();
        var result = await _presenter.PlaceOrderAsync(
            customerId,
            new List<CartItemSession>(),
            request,
            request.PaymentMethodId ?? "");

        if (result == null) return BadRequest("Checkout failed");
        return Ok(result);
    }

    private Guid GetCustomerId()
    {
        var claim = User.FindFirst("customerId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
