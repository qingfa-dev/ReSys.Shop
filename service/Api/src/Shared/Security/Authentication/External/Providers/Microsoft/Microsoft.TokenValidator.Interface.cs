namespace Shared.Security.Authentication.External.Providers.Microsoft;

public interface IMicrosoftTokenValidator
{
    Task<MicrosoftUserInfo> ValidateAsync(string accessToken, CancellationToken ct = default);
}

public sealed record MicrosoftUserInfo(string Id, string Mail, string? DisplayName);
