namespace Shared.Security.Authentication.External.Providers.Facebook;

public interface IFacebookTokenValidator
{
    Task<FacebookUserInfo> ValidateAsync(string accessToken, CancellationToken ct = default);
}

public sealed record FacebookUserInfo
{
    public string Id { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string? Name { get; init; }
}
