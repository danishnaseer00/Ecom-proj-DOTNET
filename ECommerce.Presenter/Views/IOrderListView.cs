using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Views;

public interface IOrderListView
{
    void DisplayOrders(OrderListViewModel model);
    void DisplayError(string message);
}

public interface IOrderDetailView
{
    void DisplayOrder(OrderViewModel model);
    void DisplayError(string message);
}
