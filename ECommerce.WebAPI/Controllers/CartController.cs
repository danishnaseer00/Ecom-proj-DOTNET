using ECommerce.Model.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IRepository<Model.Entities.Cart> _cartRepo;

    public CartController(ICustomerRepository customerRepo, IRepository<Model.Entities.Cart> cartRepo)
    {
        _customerRepo = customerRepo;
        _cartRepo = cartRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var customerId = GetCustomerId();
        var customer = await _customerRepo.GetByUserIdAsync(customerId.ToString());
        if (customer?.Cart == null) return Ok(new { Items = new List<object>() });
        return Ok(customer.Cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var customerId = GetCustomerId();
        var customer = await _customerRepo.GetByUserIdAsync(customerId.ToString());
        if (customer == null) return Unauthorized();

        customer.Cart ??= new Model.Entities.Cart { CustomerId = customer.Id };
        var item = customer.Cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item != null)
            item.Quantity += request.Quantity;
        else
            customer.Cart.Items.Add(new Model.Entities.CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });

        if (customer.Cart.Id == Guid.Empty)
            await _cartRepo.AddAsync(customer.Cart);
        else
            await _cartRepo.UpdateAsync(customer.Cart);

        return Ok();
    }

    private Guid GetCustomerId()
    {
        var claim = User.FindFirst("customerId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}

public record AddToCartRequest(Guid ProductId, int Quantity);
