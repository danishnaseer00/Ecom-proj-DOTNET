using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Views;

public interface IProductListView
{
    void DisplayProducts(ProductListViewModel model);
    void DisplayError(string message);
}
