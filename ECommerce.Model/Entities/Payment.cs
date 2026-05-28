using ECommerce.Model.Enums;

namespace ECommerce.Model.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
}
