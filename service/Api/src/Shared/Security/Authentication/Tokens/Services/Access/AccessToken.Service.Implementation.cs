using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Options;

namespace Shared.Security.Authentication.Tokens.Services.Access;

/// <summary>
/// Service for generating JWT access tokens using HMAC-SHA256 signing.
/// </summary>
public class AccessTokenService(IOptions<JwtSettings> jwtOptions) : IAccessTokenService
{
    private readonly JwtSettings _jwtOptions = jwtOptions.Value;

    /// <inheritdoc/>
    public Result<TokenResponseModel> GenerateToken(TokenRequestModel request)
    {
        // Validate: Ensure the signing secret meets minimum security requirements
        if (string.IsNullOrEmpty(_jwtOptions.Secret) || _jwtOptions.Secret.Length < JwtSettingsConstant.Constraints.Secret.MinLength)
            return AccessTokenResult.Failure.InvalidConfiguration;

        try
        {
            // Initialize: Create the symmetric security key from the configured secret
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            SigningCredentials credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            // Generate: Create the standard JWT claims for the user
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

            // Compute: Determine expiration timestamp based on configuration
            DateTime expiration = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);

            // Create: Build the security token descriptor with claims and signing
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            };

            // Call: Serialize the JWT to its final compact string representation
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string accessToken = tokenHandler.WriteToken(securityToken);

            // Transform: Map the generated token and expiry to the response DTO
            return Result<TokenResponseModel>.Ok(new TokenResponseModel(
                Token: accessToken,
                ExpiresIn: new DateTimeOffset(expiration).ToUnixTimeSeconds()
            ));
        }
        catch (Exception)
        {
            // Log: Cryptographic or configuration error prevented token generation
            return AccessTokenResult.Failure.GenerationFailed;
        }
    }
}
