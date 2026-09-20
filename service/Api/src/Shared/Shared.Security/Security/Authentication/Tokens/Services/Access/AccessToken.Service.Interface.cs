using Shared.Security.Authentication.Tokens.Models;

namespace Shared.Security.Authentication.Tokens.Services.Access;

/// <summary>
/// Service for generating JWT access tokens.
/// </summary>
public interface IAccessTokenService
{
    /// <summary>
    /// Generates an access token based on the provided request.
    /// </summary>
    /// <param name="request">The token request containing user information.</param>
    /// <returns>A result containing the generated access token response or error details.</returns>
    Result<TokenResponseModel> GenerateToken(TokenRequestModel request);
}
