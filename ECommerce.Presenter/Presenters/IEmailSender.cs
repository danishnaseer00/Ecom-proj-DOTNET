namespace ECommerce.Presenter.Presenters;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}
