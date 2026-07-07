namespace Shared.Security.Authentication.External.Providers.Facebook;

public interface IFacebookTokenValidator
{
    Task<FacebookUserInfo> ValidateAsync(string accessToken, CancellationToken ct = default);
}

public sealed record FacebookUserInfo(string Id, string Email, string? Name);
