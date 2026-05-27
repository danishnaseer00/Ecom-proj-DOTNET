using ECommerce.Model.Enums;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Presenters;

public class AdminOrderPresenter
{
    private readonly IRepository<Model.Entities.Order> _orderRepo;

    public AdminOrderPresenter(IRepository<Model.Entities.Order> orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public async Task<OrderListViewModel> GetAllOrdersAsync()
    {
        var orders = await _orderRepo.GetAllAsync();
        return new OrderListViewModel
        {
            Orders = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemViewModel
                {
                    ProductName = i.Product?.Name ?? "Unknown",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToList()
        };
    }

    public async Task<OrderViewModel?> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) return null;

        return new OrderViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemViewModel
            {
                ProductName = i.Product?.Name ?? "Unknown",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, string newStatus)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) return;

        if (Enum.TryParse<OrderStatus>(newStatus, true, out var status))
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(order);
        }
    }
}
