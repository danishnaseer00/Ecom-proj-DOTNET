using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Presenter.Presenters;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ResendEmailSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Resend:ApiKey"] ?? "";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var payload = new
        {
            from = "onboarding@resend.dev",
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
