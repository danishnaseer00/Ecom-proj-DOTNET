using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Presenters;

public class CartPresenter
{
    private readonly IRepository<Product> _productRepo;

    public CartPresenter(IRepository<Product> productRepo)
    {
        _productRepo = productRepo;
    }

    public CartViewModel GetCart(List<CartItemSession> sessionItems)
    {
        if (sessionItems == null || sessionItems.Count == 0)
            return new CartViewModel();

        var productIds = sessionItems.Select(i => i.ProductId).ToList();
        var products = _productRepo.FindAsync(p => productIds.Contains(p.Id)).Result;

        var items = sessionItems.Select(si =>
        {
            var product = products.FirstOrDefault(p => p.Id == si.ProductId);
            return new CartItemViewModel
            {
                ProductId = si.ProductId,
                ProductName = product?.Name ?? "Unknown",
                UnitPrice = product?.Price ?? 0,
                Quantity = si.Quantity,
                ImageUrl = product?.ImageUrl
            };
        }).ToList();

        return new CartViewModel { Items = items };
    }

    public List<CartItemSession> AddToCart(List<CartItemSession> sessionItems, Guid productId, int quantity = 1)
    {
        sessionItems ??= new List<CartItemSession>();

        var existing = sessionItems.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
            existing.Quantity += quantity;
        else
            sessionItems.Add(new CartItemSession { ProductId = productId, Quantity = quantity });

        return sessionItems;
    }

    public List<CartItemSession> RemoveFromCart(List<CartItemSession> sessionItems, Guid productId)
    {
        sessionItems?.RemoveAll(i => i.ProductId == productId);
        return sessionItems ?? new List<CartItemSession>();
    }

    public List<CartItemSession> UpdateQuantity(List<CartItemSession> sessionItems, Guid productId, int quantity)
    {
        var item = sessionItems?.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
                sessionItems!.Remove(item);
            else
                item.Quantity = quantity;
        }
        return sessionItems ?? new List<CartItemSession>();
    }
}

public class CartItemSession
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
