using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Views;

public interface ICheckoutView
{
    void DisplayCheckout(CheckoutViewModel model);
    void DisplayError(string message);
    void RedirectToOrderConfirmation(Guid orderId);
}
