namespace Shared.Security.Authentication.External.Providers.Microsoft;

public interface IMicrosoftTokenValidator
{
    Task<MicrosoftUserInfo> ValidateAsync(string accessToken, CancellationToken ct = default);
}

public sealed record MicrosoftUserInfo
{
    public string Id { get; init; } = default!;
    public string Mail { get; init; } = default!;
    public string? DisplayName { get; init; }
}
