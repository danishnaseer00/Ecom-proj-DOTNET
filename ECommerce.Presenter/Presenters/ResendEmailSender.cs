using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Presenter.Presenters;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public ResendEmailSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Resend:ApiKey"] ?? "";
        _fromEmail = config["Resend:FromEmail"] ?? "onboarding@resend.dev";
        _fromName = config["Resend:FromName"] ?? "E-Commerce Store";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var payload = new
        {
            from = $"{_fromName} <{_fromEmail}>",
            to = new[] { to },
            subject,
            html = body
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = JsonContent.Create(payload)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
