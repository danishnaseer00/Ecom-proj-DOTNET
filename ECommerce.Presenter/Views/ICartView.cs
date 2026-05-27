using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Views;

public interface ICartView
{
    void DisplayCart(CartViewModel model);
    void DisplayError(string message);
    void RedirectToCheckout();
}
