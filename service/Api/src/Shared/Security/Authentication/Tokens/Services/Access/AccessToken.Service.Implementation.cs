using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Options;

namespace Shared.Security.Authentication.Tokens.Services.Access;

/// <summary>Produces a signed JWT access token using HMAC-SHA256 with configurable issuer, audience, and expiration.</summary>
// Invariant: Secret length >= 32 chars minimum; signing always uses HMAC-SHA256; JTI uniquely identifies each token.
// Context: JWT signing secret must never leak to logs or error responses — see Threat TMT-TOK-001.
public class AccessTokenService(IOptions<JwtSettings> jwtOptions) : IAccessTokenService
{
    private readonly JwtSettings _jwtOptions = jwtOptions.Value;

    /// <summary>Generates a signed JWT access token from the provided user request model.</summary>
    // Contract: pre=request!=null && _jwtOptions.Secret.Length>=JwtSettingsConstant.Constraints.Secret.MinLength, post=return.IsSuccess && TokenResponseModel.Token!=null, throws=Exception on cryptographic failure
    public Result<TokenResponseModel> GenerateToken(TokenRequestModel request)
    {
        // Validate: signing secret meets minimum length before any crypto operation to prevent weak-key attacks
        if (string.IsNullOrEmpty(_jwtOptions.Secret) || _jwtOptions.Secret.Length < JwtSettingsConstant.Constraints.Secret.MinLength)
            return AccessTokenResult.Failure.InvalidConfiguration;

        try
        {
            // Compute: symmetric key from configured secret for HMAC-SHA256 signing
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            SigningCredentials credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            // Transform: user identity into standard JWT registered claims for downstream service consumption
            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, request.Email),
                new Claim(JwtRegisteredClaimNames.Name, request.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer64)
            ];

            // Compute: absolute expiration from configuration to enforce token lifetime policy
            DateTime expiration = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);

            // Create: token descriptor with claims, expiry, signing — all inputs must be populated
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            };

            // Call: JwtSecurityTokenHandler serializes into compact token string for HTTP bearer transmission
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string accessToken = tokenHandler.WriteToken(securityToken);

            // Transform: domain token and expiry into wire-format response DTO
            return Result<TokenResponseModel>.Ok(new TokenResponseModel(
                Token: accessToken,
                ExpiresIn: new DateTimeOffset(expiration).ToUnixTimeSeconds()
            ));
        }
        catch (Exception ex)
        {
            // Catch: Cryptographic failure must not propagate stack details to caller — generic error only, see TMT-TOK-002
            return Result<TokenResponseModel>.Unexpected(
                exception: ex,
                errors: [AccessTokenResult.Failure.GenerationFailed]);
        }
    }
}
