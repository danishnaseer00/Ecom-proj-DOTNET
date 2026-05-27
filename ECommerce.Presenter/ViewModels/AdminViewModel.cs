namespace ECommerce.Presenter.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal Revenue { get; set; }
    public int LowStockCount { get; set; }
    public List<ProductViewModel> LowStockProducts { get; set; } = new();
}

public class CustomerViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
