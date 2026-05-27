using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Views;

public interface IDashboardView
{
    void DisplayDashboard(DashboardViewModel model);
}

public interface IAdminProductListView
{
    void DisplayProducts(ProductListViewModel model);
    void DisplayError(string message);
}

public interface IAdminOrderListView
{
    void DisplayOrders(OrderListViewModel model);
    void DisplayError(string message);
}

public interface ICustomerListView
{
    void DisplayCustomers(List<CustomerViewModel> customers);
    void DisplayError(string message);
}
