using ECommerce.Model.Entities;
using ECommerce.Model.Enums;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Presenters;

public class AdminDashboardPresenter
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Model.Entities.Order> _orderRepo;
    private readonly IRepository<Customer> _customerRepo;

    public AdminDashboardPresenter(
        IRepository<Product> productRepo,
        IRepository<Model.Entities.Order> orderRepo,
        IRepository<Customer> customerRepo)
    {
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var products = await _productRepo.GetAllAsync();
        var orders = await _orderRepo.GetAllAsync();
        var customers = await _customerRepo.GetAllAsync();

        var lowStockProducts = products.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 5).ToList();

        return new DashboardViewModel
        {
            TotalProducts = products.Count,
            TotalOrders = orders.Count,
            TotalCustomers = customers.Count,
            Revenue = orders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid)
                           .Sum(o => o.TotalAmount),
            LowStockCount = lowStockProducts.Count,
            LowStockProducts = lowStockProducts.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                StockQuantity = p.StockQuantity,
                Price = p.Price
            }).ToList()
        };
    }

    public async Task<List<CustomerViewModel>> GetAllCustomersAsync()
    {
        var customers = await _customerRepo.GetAllAsync();
        return customers.Select(c => new CustomerViewModel
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}
