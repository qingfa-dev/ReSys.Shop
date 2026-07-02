using Shared.Security.Authentication.Tokens.Models;

namespace Shared.UnitTests.Security.Authentication.Tokens.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenModels")]
public sealed class TokenResponseModelTests
{
    [Fact(DisplayName = "TokenResponseModel should set all properties from constructor")]
    public void TokenResponseModel_Constructor_SetsProperties()
    {
        // Arrange
        string token = "jwt.token.here";
        long expiresIn = 1234567890;

        // Act
        TokenResponseModel model = new(token, expiresIn);

        // Assert
        model.Token.Should().Be(token);
        model.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact(DisplayName = "TokenResponseModel should have structural equality")]
    public void TokenResponseModel_ShouldHaveStructuralEquality()
    {
        // Arrange
        TokenResponseModel model1 = new("token-a", 1000);
        TokenResponseModel model2 = new("token-a", 1000);
        TokenResponseModel model3 = new("token-b", 2000);

        // Assert
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }

    [Fact(DisplayName = "RefreshTokenResponseModel should set all properties")]
    public void RefreshTokenResponseModel_Constructor_SetsProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string token = "refresh-token-value";
        Guid userId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow.AddDays(-1);
        DateTime expiresAt = DateTime.UtcNow.AddDays(7);
        DateTime? revokedAt = null;
        string? revokedReason = null;
        string? replacedByToken = null;

        // Act
        RefreshTokenResponseModel model = new(
            id, token, userId, createdAt, expiresAt,
            revokedAt, revokedReason, replacedByToken, IsActive: true);

        // Assert
        model.Id.Should().Be(id);
        model.Token.Should().Be(token);
        model.UserId.Should().Be(userId);
        model.CreatedAt.Should().Be(createdAt);
        model.ExpiresAt.Should().Be(expiresAt);
        model.RevokedAt.Should().BeNull();
        model.RevokedReason.Should().BeNull();
        model.ReplacedByToken.Should().BeNull();
        model.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "RefreshTokenResponseModel should set revoked properties when token is revoked")]
    public void RefreshTokenResponseModel_WhenRevoked_SetsRevokedProperties()
    {
        // Arrange
        DateTime revokedAt = DateTime.UtcNow;
        string revokedReason = "user_logout";

        // Act
        RefreshTokenResponseModel model = new(
            Guid.NewGuid(), "token", Guid.NewGuid(),
            DateTime.UtcNow, DateTime.UtcNow.AddDays(7),
            revokedAt, revokedReason, null, IsActive: false);

        // Assert
        model.RevokedAt.Should().Be(revokedAt);
        model.RevokedReason.Should().Be(revokedReason);
        model.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "RefreshTokenResponseModel should have structural equality")]
    public void RefreshTokenResponseModel_ShouldHaveStructuralEquality()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        RefreshTokenResponseModel model1 = new(
            id, "tok", userId, now, now.AddDays(1),
            null, null, null, IsActive: true);

        RefreshTokenResponseModel model2 = new(
            id, "tok", userId, now, now.AddDays(1),
            null, null, null, IsActive: true);

        RefreshTokenResponseModel model3 = new(
            Guid.NewGuid(), "other", Guid.NewGuid(), now, now.AddDays(1),
            null, null, null, IsActive: false);

        // Assert
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }
}
