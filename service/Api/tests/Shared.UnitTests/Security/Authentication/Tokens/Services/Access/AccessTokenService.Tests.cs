using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Access;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Access;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "AccessTokenService")]
public sealed class AccessTokenServiceTests
{
    private static JwtSettings CreateValidSettings()
    {
        return new JwtSettings
        {
            Secret = "super-secret-key-that-is-long-enough-for-testing-123!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationInMinutes = 15
        };
    }

    private static AccessTokenService CreateService(JwtSettings? settings = null)
    {
        IOptions<JwtSettings> options = Microsoft.Extensions.Options.Options.Create(settings ?? CreateValidSettings());
        return new AccessTokenService(options);
    }

    [Fact(DisplayName = "GenerateToken should return valid token response for valid request")]
    public void GenerateToken_ReturnsTokenResponse_ForValidRequest()
    {
        // Arrange
        AccessTokenService service = CreateService();
        TokenRequestModel request = new TokenRequestModel(
            UserId: Guid.NewGuid(),
            Email: "test@example.com",
            FullName: "Test User");

        // Act
        Result<TokenResponseModel> result = service.GenerateToken(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.Token.Split('.').Should().HaveCount(3, "JWT must have header, payload, and signature");
        result.Value.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "GenerateToken should contain expected JWT claims")]
    public void GenerateToken_ReturnsExpectedClaims()
    {
        // Arrange
        AccessTokenService service = CreateService();
        Guid userId = Guid.NewGuid();
        TokenRequestModel request = new TokenRequestModel(
            UserId: userId,
            Email: "claims@example.com",
            FullName: "Claims User");

        // Act
        Result<TokenResponseModel> result = service.GenerateToken(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt = handler.ReadJwtToken(result.Value.Token);

        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "claims@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "name" && c.Value == "Claims User");
        jwt.Claims.Should().Contain(c => c.Type == "jti");
        jwt.Claims.Should().Contain(c => c.Type == "iat");
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }

    [Fact(DisplayName = "GenerateToken should set correct expiration time")]
    public void GenerateToken_ReturnsCorrectExpiration()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.AccessTokenExpirationInMinutes = 15;
        AccessTokenService service = CreateService(settings);
        TokenRequestModel request = new TokenRequestModel(
            UserId: Guid.NewGuid(),
            Email: "expiry@example.com",
            FullName: "Expiry User");

        // Act
        Result<TokenResponseModel> result = service.GenerateToken(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        long expectedExpiresIn = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();
        long actualExpiresIn = result.Value.ExpiresIn;

        actualExpiresIn.Should().BeInRange(
            expectedExpiresIn - 2,
            expectedExpiresIn + 2,
            "expiration should be 15 minutes from now (±2 seconds tolerance)");
    }

    [Fact(DisplayName = "GenerateToken should return invalid configuration when secret is too short")]
    public void GenerateToken_ReturnsInvalidConfiguration_WhenSecretTooShort()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Secret = "short";
        AccessTokenService service = CreateService(settings);
        TokenRequestModel request = new TokenRequestModel(
            UserId: Guid.NewGuid(),
            Email: "test@example.com",
            FullName: "Test User");

        // Act
        Result<TokenResponseModel> result = service.GenerateToken(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "AccessToken.InvalidConfiguration");
    }

    [Fact(DisplayName = "GenerateToken should return invalid configuration when secret is empty")]
    public void GenerateToken_ReturnsInvalidConfiguration_WhenSecretEmpty()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Secret = string.Empty;
        AccessTokenService service = CreateService(settings);
        TokenRequestModel request = new TokenRequestModel(
            UserId: Guid.NewGuid(),
            Email: "test@example.com",
            FullName: "Test User");

        // Act
        Result<TokenResponseModel> result = service.GenerateToken(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "AccessToken.InvalidConfiguration");
    }
}
