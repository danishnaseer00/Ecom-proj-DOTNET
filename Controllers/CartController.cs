using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ecommerce_web_project.Controllers;

public class CartController : Controller
{
    private readonly CartPresenter _presenter;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICustomerRepository _customerRepo;
    private readonly IRepository<Cart> _cartRepo;
    private readonly IRepository<CartItem> _cartItemRepo;

    public CartController(
        CartPresenter presenter,
        UserManager<IdentityUser> userManager,
        ICustomerRepository customerRepo,
        IRepository<Cart> cartRepo,
        IRepository<CartItem> cartItemRepo)
    {
        _presenter = presenter;
        _userManager = userManager;
        _customerRepo = customerRepo;
        _cartRepo = cartRepo;
        _cartItemRepo = cartItemRepo;
    }

    private async Task<List<CartItemSession>> GetEffectiveCartAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var customer = await _customerRepo.GetByUserIdAsync(user.Id);
                if (customer != null)
                {
                    var dbCart = (await _cartRepo.FindAsync(c => c.CustomerId == customer.Id)).FirstOrDefault();
                    if (dbCart != null)
                    {
                        var dbItems = await _cartItemRepo.FindAsync(i => i.CartId == dbCart.Id);
                        return dbItems.Select(i => new CartItemSession { ProductId = i.ProductId, Quantity = i.Quantity }).ToList();
                    }
                    return new List<CartItemSession>();
                }
            }
        }
        return JsonSerializer.Deserialize<List<CartItemSession>>(HttpContext.Session.GetString("Cart") ?? "[]") ?? new();
    }

    private async Task SaveEffectiveCartAsync(List<CartItemSession> items)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var customer = await _customerRepo.GetByUserIdAsync(user.Id);
                if (customer != null)
                {
                    var dbCart = (await _cartRepo.FindAsync(c => c.CustomerId == customer.Id)).FirstOrDefault();
                    if (dbCart == null)
                    {
                        dbCart = new Cart { CustomerId = customer.Id };
                        await _cartRepo.AddAsync(dbCart);
                    }
                    var existingItems = await _cartItemRepo.FindAsync(i => i.CartId == dbCart.Id);
                    foreach (var old in existingItems)
                        await _cartItemRepo.DeleteAsync(old);
                    foreach (var item in items)
                        await _cartItemRepo.AddAsync(new CartItem { CartId = dbCart.Id, ProductId = item.ProductId, Quantity = item.Quantity });
                    return;
                }
            }
        }
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(items));
    }

    public async Task<IActionResult> Index()
    {
        var items = await GetEffectiveCartAsync();
        var cart = _presenter.GetCart(items);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
    {
        var items = await GetEffectiveCartAsync();
        items = _presenter.AddToCart(items, productId, quantity);
        await SaveEffectiveCartAsync(items);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(Guid productId, int quantity)
    {
        var items = await GetEffectiveCartAsync();
        items = _presenter.UpdateQuantity(items, productId, quantity);
        await SaveEffectiveCartAsync(items);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(Guid productId)
    {
        var items = await GetEffectiveCartAsync();
        items = _presenter.RemoveFromCart(items, productId);
        await SaveEffectiveCartAsync(items);
        return RedirectToAction("Index");
    }
}
