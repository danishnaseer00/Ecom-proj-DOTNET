using ECommerce.Presenter.Presenters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ecommerce_web_project.Controllers;

public class CartController : Controller
{
    private readonly CartPresenter _presenter;

    public CartController(CartPresenter presenter) => _presenter = presenter;

    public IActionResult Index()
    {
        var sessionItems = GetCart();
        var cart = _presenter.GetCart(sessionItems);
        return View(cart);
    }

    [HttpPost]
    public IActionResult AddToCart(Guid productId, int quantity = 1)
    {
        var sessionItems = GetCart();
        sessionItems = _presenter.AddToCart(sessionItems, productId, quantity);
        SaveCart(sessionItems);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpdateQuantity(Guid productId, int quantity)
    {
        var sessionItems = GetCart();
        sessionItems = _presenter.UpdateQuantity(sessionItems, productId, quantity);
        SaveCart(sessionItems);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Remove(Guid productId)
    {
        var sessionItems = GetCart();
        sessionItems = _presenter.RemoveFromCart(sessionItems, productId);
        SaveCart(sessionItems);
        return RedirectToAction("Index");
    }

    private List<CartItemSession> GetCart() =>
        JsonSerializer.Deserialize<List<CartItemSession>>(HttpContext.Session.GetString("Cart") ?? "[]") ?? new();

    private void SaveCart(List<CartItemSession> cart) =>
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
}
