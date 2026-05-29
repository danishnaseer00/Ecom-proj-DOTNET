namespace ECommerce.Presenter.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal Revenue { get; set; }
    public int LowStockCount { get; set; }
    public List<ProductViewModel> LowStockProducts { get; set; } = new();

    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
    public List<DailyRevenue> DailyRevenues { get; set; } = new();
    public List<ProductSales> TopProducts { get; set; } = new();
    public Dictionary<string, int> ProductsByCategory { get; set; } = new();
}

public class MonthlyRevenue
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class DailyRevenue
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class ProductSales
{
    public string Name { get; set; } = string.Empty;
    public int TotalSold { get; set; }
}

public class CustomerViewModel
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
