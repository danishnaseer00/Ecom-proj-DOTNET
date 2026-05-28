namespace ECommerce.Presenter.ViewModels;

public class ReviewViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewRequest
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
