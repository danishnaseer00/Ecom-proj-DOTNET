namespace ECommerce.Presenter.ViewModels;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}

public class ProductListViewModel
{
    public IReadOnlyList<ProductViewModel> Products { get; set; } = Array.Empty<ProductViewModel>();
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public IReadOnlyList<CategoryViewModel> Categories { get; set; } = Array.Empty<CategoryViewModel>();
}
