using Microsoft.Extensions.Logging;

namespace ECommerce.Presenter.Presenters;

public class NoopEmailSender : IEmailSender
{
    private readonly ILogger<NoopEmailSender> _logger;

    public NoopEmailSender(ILogger<NoopEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Email not sent (noop): To={To}, Subject={Subject}", to, subject);
        return Task.CompletedTask;
    }
}
