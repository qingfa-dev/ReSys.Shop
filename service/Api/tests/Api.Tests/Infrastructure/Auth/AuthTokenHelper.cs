using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Api.Tests.Infrastructure.Auth;

public static class AuthTokenHelper
{
    private const string TestSecret = "integration-test-secret-key-32-chars!!";
    private const string TestIssuer = "ReSys.Shop.Test";
    private const string TestAudience = "ReSys.Shop.Test";

    private static readonly Lazy<string> _cachedAdminToken = new(BuildAdminToken);

    public static string GenerateAdminToken() => _cachedAdminToken.Value;

    private static string BuildAdminToken()
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestSecret));
        SigningCredentials credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, "admin@test.com"),
            new Claim(JwtRegisteredClaimNames.Name, "Admin User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new Claim("role", "Admin")
        ];

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
}
