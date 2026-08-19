using Module.Identity.Features.Storefront.Shared.Models;
using Shared.Security.Authentication.Tokens.Models;

namespace Module.Identity.Features.Storefront.Shared.Mappings;

/// <summary>
/// Maps an access-token / refresh-token pair onto an auth-token response model.
/// </summary>
public static class AuthTokenMapping
{
    /// <summary>
    /// Maps the token pair onto a response deriving from <see cref="BaseTokenResponseModel"/>.
    /// </summary>
    public static T MapToTokenResponse<T>(
        this (TokenResponseModel AccessToken, RefreshTokenResponseModel RefreshToken) source)
        where T : BaseTokenResponseModel, new()
        => new T
        {
            AccessToken = source.AccessToken.Token,
            AccessTokenExpiresIn = source.AccessToken.ExpiresIn,
            RefreshToken = source.RefreshToken.Token,
            RefreshTokenExpiresIn = source.RefreshToken.ExpiresAt.ToUnixTimeSeconds()
        };
}
