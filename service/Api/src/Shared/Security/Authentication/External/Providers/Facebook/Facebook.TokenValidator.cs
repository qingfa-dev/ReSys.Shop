using System.Net.Http.Json;
using System.Text.Json;

namespace Shared.Security.Authentication.External.Providers.Facebook;

public sealed partial class FacebookTokenValidator : IFacebookTokenValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FacebookTokenValidator> _logger;

    public FacebookTokenValidator(IHttpClientFactory httpClientFactory, ILogger<FacebookTokenValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<FacebookUserInfo> ValidateAsync(string accessToken, CancellationToken ct = default)
    {
        using var http = _httpClientFactory.CreateClient("FacebookGraph");
        var url = $"https://graph.facebook.com/me?fields=id,email,name&access_token={Uri.EscapeDataString(accessToken)}";
        var response = await http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            Loggers.ValidationFailed(_logger, response.StatusCode);
            throw new InvalidOperationException("Invalid Facebook access token");
        }
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return new FacebookUserInfo(
            Id: doc.GetProperty("id").GetString() ?? string.Empty,
            Email: doc.TryGetProperty("email", out var e) ? e.GetString() ?? string.Empty : string.Empty,
            Name: doc.TryGetProperty("name", out var n) ? n.GetString() : null);
    }
}
