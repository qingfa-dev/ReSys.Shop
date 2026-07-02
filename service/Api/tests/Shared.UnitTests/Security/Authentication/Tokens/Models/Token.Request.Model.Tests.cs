using Shared.Security.Authentication.Tokens.Models;

namespace Shared.UnitTests.Security.Authentication.Tokens.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenModels")]
public sealed class TokenRequestModelTests
{
    [Fact(DisplayName = "TokenRequestModel should set all properties from constructor")]
    public void TokenRequestModel_Constructor_SetsProperties()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string email = "test@example.com";
        string fullName = "Test User";

        // Act
        TokenRequestModel model = new(userId, email, fullName);

        // Assert
        model.UserId.Should().Be(userId);
        model.Email.Should().Be(email);
        model.FullName.Should().Be(fullName);
    }

    [Fact(DisplayName = "TokenRequestModel should have structural equality")]
    public void TokenRequestModel_ShouldHaveStructuralEquality()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        TokenRequestModel model1 = new(userId, "a@b.com", "User A");
        TokenRequestModel model2 = new(userId, "a@b.com", "User A");
        TokenRequestModel model3 = new(Guid.NewGuid(), "c@d.com", "User C");

        // Assert
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }

    [Fact(DisplayName = "RevokeTokenRequestModel should set Token and Reason")]
    public void RevokeTokenRequestModel_Constructor_SetsProperties()
    {
        // Arrange
        string token = "refresh-token-value";
        string reason = "user_logout";

        // Act
        RevokeTokenRequestModel model = new(token, reason);

        // Assert
        model.Token.Should().Be(token);
        model.Reason.Should().Be(reason);
    }

    [Fact(DisplayName = "RevokeTokenRequestModel should default Reason to null")]
    public void RevokeTokenRequestModel_DefaultReason_ShouldBeNull()
    {
        // Act
        RevokeTokenRequestModel model = new("some-token");

        // Assert
        model.Reason.Should().BeNull();
    }

    [Fact(DisplayName = "RevokeTokenRequestModel should have structural equality")]
    public void RevokeTokenRequestModel_ShouldHaveStructuralEquality()
    {
        // Arrange
        RevokeTokenRequestModel model1 = new("token-1", "logout");
        RevokeTokenRequestModel model2 = new("token-1", "logout");
        RevokeTokenRequestModel model3 = new("token-2", "logout");

        // Assert
        model1.Should().Be(model2);
        model1.Should().NotBe(model3);
    }
}
