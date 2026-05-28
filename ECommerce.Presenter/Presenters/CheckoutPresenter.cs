using ECommerce.Model.Entities;
using ECommerce.Model.Enums;
using ECommerce.Model.Repositories;
using ECommerce.Model.ValueObjects;
using ECommerce.Presenter.ViewModels;
using Stripe;
using ProductEntity = ECommerce.Model.Entities.Product;
using AddressValue = ECommerce.Model.ValueObjects.Address;

namespace ECommerce.Presenter.Presenters;

public class CheckoutPresenter
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IRepository<ProductEntity> _productRepo;

    public CheckoutPresenter(
        IRepository<Order> orderRepo,
        IRepository<Payment> paymentRepo,
        ICustomerRepository customerRepo,
        IRepository<ProductEntity> productRepo)
    {
        _orderRepo = orderRepo;
        _paymentRepo = paymentRepo;
        _customerRepo = customerRepo;
        _productRepo = productRepo;
    }

    public async Task<CheckoutViewModel> PrepareCheckoutAsync(string? customerId, List<CartItemSession> sessionItems)
    {
        var cartPresenter = new CartPresenter(_productRepo);
        var cart = cartPresenter.GetCart(sessionItems);

        if (cart.Items.Count == 0)
            return new CheckoutViewModel { ErrorMessage = "Cart is empty" };

        var total = (long)(cart.Total * 100);

        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = total,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return new CheckoutViewModel
            {
                Cart = cart,
                StripeClientSecret = intent.ClientSecret
            };
        }
        catch (Exception ex)
        {
            return new CheckoutViewModel { ErrorMessage = $"Payment error: {ex.Message}" };
        }
    }

    public async Task<OrderViewModel?> PlaceOrderAsync(
        string? customerId,
        List<CartItemSession> sessionItems,
        CheckoutRequest request,
        string paymentIntentId)
    {
        var cartPresenter = new CartPresenter(_productRepo);
        var cart = cartPresenter.GetCart(sessionItems);
        if (cart.Items.Count == 0) return null;

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20],
            CustomerId = Guid.Parse(customerId ?? Guid.NewGuid().ToString()),
            Status = OrderStatus.Paid,
            TotalAmount = cart.Total,
            ShippingAddress = new AddressValue
            {
                Street = request.Street ?? "",
                City = request.City ?? "",
                State = request.State ?? "",
                ZipCode = request.ZipCode ?? "",
                Country = request.Country ?? ""
            }
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        await _orderRepo.AddAsync(order);

        var payment = new Payment
        {
            OrderId = order.Id,
            StripePaymentIntentId = paymentIntentId,
            Status = PaymentStatus.Succeeded,
            Amount = cart.Total,
            Currency = "usd"
        };
        await _paymentRepo.AddAsync(payment);

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

    public async Task<bool> HandleStripeWebhookAsync(string json, string signatureHeader, string webhookSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var intent = stripeEvent.Data.Object as PaymentIntent;
                if (intent != null)
                {
                    var payments = await _paymentRepo.FindAsync(p => p.StripePaymentIntentId == intent.Id);
                    var payment = payments.FirstOrDefault();
                    if (payment != null)
                    {
                        payment.Status = PaymentStatus.Succeeded;
                        await _paymentRepo.UpdateAsync(payment);

                        var order = await _orderRepo.GetByIdAsync(payment.OrderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Paid;
                            await _orderRepo.UpdateAsync(order);
                        }
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
