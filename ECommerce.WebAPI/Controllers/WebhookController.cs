using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly CheckoutPresenter _presenter;
    private readonly IConfiguration _config;

    public WebhookController(CheckoutPresenter presenter, IConfiguration config)
    {
        _presenter = presenter;
        _config = config;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";
        var secret = _config["Stripe:WebhookSecret"] ?? "";

        var success = await _presenter.HandleStripeWebhookAsync(json, signature, secret);
        if (!success) return BadRequest();
        return Ok();
    }
}
