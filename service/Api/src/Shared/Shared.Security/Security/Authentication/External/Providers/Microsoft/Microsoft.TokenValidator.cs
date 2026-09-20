using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shared.Security.Authentication.External.Providers.Microsoft;

public sealed partial class MicrosoftTokenValidator : IMicrosoftTokenValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicrosoftTokenValidator> _logger;

    public MicrosoftTokenValidator(IHttpClientFactory httpClientFactory, ILogger<MicrosoftTokenValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<MicrosoftUserInfo> ValidateAsync(string accessToken, CancellationToken ct = default)
    {
        using var http = _httpClientFactory.CreateClient("MicrosoftGraph");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var url = "https://graph.microsoft.com/v1.0/me";
        var response = await http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            Loggers.ValidationFailed(_logger, response.StatusCode);
            throw new InvalidOperationException("Invalid Microsoft access token");
        }
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return new MicrosoftUserInfo
        {
            Id = doc.GetProperty("id").GetString() ?? string.Empty,
            Mail = doc.TryGetProperty("mail", out var m) ? m.GetString() ?? string.Empty
                : (doc.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() ?? string.Empty : string.Empty),
            DisplayName = doc.TryGetProperty("displayName", out var n) ? n.GetString() : null
        };
    }
}
