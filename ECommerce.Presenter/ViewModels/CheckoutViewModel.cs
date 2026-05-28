namespace ECommerce.Presenter.ViewModels;

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public string? StripeClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CheckoutRequest
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? PaymentMethodId { get; set; }
}
