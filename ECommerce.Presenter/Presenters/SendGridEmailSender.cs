using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Presenter.Presenters;

public class SendGridEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["SendGrid:ApiKey"] ?? "";
        _fromEmail = config["SendGrid:FromEmail"] ?? "danishnaseer000@gmail.com";
        _fromName = config["SendGrid:FromName"] ?? "DanStore";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = to } } } },
            from = new { email = _fromEmail, name = _fromName },
            subject,
            content = new[] { new { type = "text/html", value = body } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
