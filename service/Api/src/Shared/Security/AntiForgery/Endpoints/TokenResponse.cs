namespace Shared.Security.AntiForgery.Endpoints;

public sealed record TokenResponse
{
    public string Token { get; init; } = default!;
    public string HeaderName { get; init; } = default!;
}
