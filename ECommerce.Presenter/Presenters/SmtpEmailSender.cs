using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Presenter.Presenters;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SmtpEmailSender(IConfiguration config)
    {
        _host = config["Smtp:Host"] ?? "smtp.gmail.com";
        _port = int.Parse(config["Smtp:Port"] ?? "587");
        _username = config["Smtp:Username"] ?? "";
        _password = config["Smtp:Password"] ?? "";
        _fromEmail = config["Smtp:FromEmail"] ?? _username;
        _fromName = config["Smtp:FromName"] ?? "DanStore";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_host, _port)
        {
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message);
    }
}
