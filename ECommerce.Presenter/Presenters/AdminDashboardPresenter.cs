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
    private readonly IRepository<OrderItem> _orderItemRepo;
    private readonly IRepository<Category> _categoryRepo;

    public AdminDashboardPresenter(
        IRepository<Product> productRepo,
        IRepository<Model.Entities.Order> orderRepo,
        IRepository<Customer> customerRepo,
        IRepository<OrderItem> orderItemRepo,
        IRepository<Category> categoryRepo)
    {
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
        _orderItemRepo = orderItemRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var products = await _productRepo.GetAllAsync();
        var orders = await _orderRepo.GetAllAsync();
        var customers = await _customerRepo.GetAllAsync();
        var orderItems = await _orderItemRepo.GetAllAsync();
        var categories = await _categoryRepo.GetAllAsync();

        var lowStockProducts = products.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 5).ToList();

        var deliveredPaid = orders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid).ToList();

        var ordersByStatus = Enum.GetValues<OrderStatus>()
            .ToDictionary(s => s.ToString(), s => orders.Count(o => o.Status == s));

        var monthlyRevenue = deliveredPaid
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyRevenue
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .ToList();

        var topProducts = orderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .Join(products, x => x.ProductId, p => p.Id, (x, p) => new ProductSales
            {
                Name = p.Name,
                TotalSold = x.TotalSold
            })
            .ToList();

        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var productsByCategory = products
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => categoryMap.GetValueOrDefault(g.Key, "Unknown"), g => g.Count());

        return new DashboardViewModel
        {
            TotalProducts = products.Count,
            TotalOrders = orders.Count,
            TotalCustomers = customers.Count,
            Revenue = deliveredPaid.Sum(o => o.TotalAmount),
            LowStockCount = lowStockProducts.Count,
            LowStockProducts = lowStockProducts.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                StockQuantity = p.StockQuantity,
                Price = p.Price
            }).ToList(),
            OrdersByStatus = ordersByStatus,
            MonthlyRevenues = monthlyRevenue,
            TopProducts = topProducts,
            ProductsByCategory = productsByCategory
        };
    }

    public async Task<List<CustomerViewModel>> GetAllCustomersAsync()
    {
        var customers = await _customerRepo.GetAllAsync();
        return customers.Select(c => new CustomerViewModel
        {
            Id = c.Id,
            UserId = c.UserId,
            FirstName = c.FirstName,
            LastName = c.LastName,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}
