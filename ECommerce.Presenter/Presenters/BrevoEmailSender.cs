using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Presenter.Presenters;

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public BrevoEmailSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Brevo:ApiKey"] ?? "";
        _fromEmail = config["Brevo:FromEmail"] ?? "danishnaseer000@gmail.com";
        _fromName = config["Brevo:FromName"] ?? "DanStore";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var payload = new
        {
            sender = new { email = _fromEmail, name = _fromName },
            to = new[] { new { email = to } },
            subject,
            htmlContent = body
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Headers = { { "api-key", _apiKey } },
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
