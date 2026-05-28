using ECommerce.Model.Repositories;
using ECommerce.Presenter.Presenters;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Presenter;

public static class DependencyInjection
{
    public static IServiceCollection AddPresenterLayer(this IServiceCollection services)
    {
        services.AddScoped<ProductListPresenter>();
        services.AddScoped<CartPresenter>();
        services.AddScoped<CheckoutPresenter>();
        services.AddScoped<OrderPresenter>();
        services.AddScoped<AdminProductPresenter>();
        services.AddScoped<AdminDashboardPresenter>();
        services.AddScoped<AdminOrderPresenter>();
        services.AddScoped<ReviewPresenter>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
