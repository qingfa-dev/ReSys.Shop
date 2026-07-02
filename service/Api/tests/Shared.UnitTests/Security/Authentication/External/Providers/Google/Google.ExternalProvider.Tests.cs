using Google.Apis.Auth;

using Microsoft.Extensions.Logging;

using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers.Google;
using Shared.Security.Authentication.External.Providers.Google.Options;

namespace Shared.UnitTests.Security.Authentication.External.Providers.Google;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "ExternalAuth")]
public sealed class GoogleExternalProviderTests
{
    private readonly Mock<IGoogleTokenValidator> _validatorMock = new();
    private readonly GoogleOptions _options = new() { ClientId = "test-client-id.apps.googleusercontent.com" };

    private GoogleExternalProvider CreateProvider()
    {
        return new GoogleExternalProvider(
            Microsoft.Extensions.Options.Options.Create(_options),
            Mock.Of<ILogger<GoogleExternalProvider>>(),
            _validatorMock.Object);
    }

    [Fact(DisplayName = "Provider should return 'google'")]
    public void Provider_ShouldReturnGoogle()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();

        // Act
        string providerName = provider.Provider;

        // Assert
        providerName.Should().Be("google");
    }

    [Fact(DisplayName = "GetProviderConfig should return ProviderOption with client_id from options")]
    public void GetProviderConfig_ShouldReturnClientId()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();

        // Act
        ProviderOption config = provider.GetProviderConfig();

        // Assert
        config.Provider.Should().Be("google");
        config.Options.Should().ContainKey("client_id");
        config.Options["client_id"].Should().Be("test-client-id.apps.googleusercontent.com");
    }

    [Fact(DisplayName = "ValidateIdTokenAsync should return ExternalUserInfo for valid token")]
    public async Task ValidateIdTokenAsync_ValidToken_ReturnsExternalUserInfo()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();
        GoogleJsonWebSignature.Payload payload = new()
        {
            Subject = "google-sub-123",
            Email = "user@gmail.com",
            GivenName = "John",
            FamilyName = "Doe",
            Name = "John Doe"
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<GoogleJsonWebSignature.ValidationSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        Result<ExternalUserInfo> result = await provider.ValidateIdTokenAsync("valid-id-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Provider.Should().Be("google");
        result.Value.ProviderSubjectId.Should().Be("google-sub-123");
        result.Value.Email.Should().Be("user@gmail.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
    }

    [Fact(DisplayName = "ValidateIdTokenAsync should use email prefix as FirstName when GivenName is null")]
    public async Task ValidateIdTokenAsync_WhenGivenNameNull_UsesEmailPrefix()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();
        GoogleJsonWebSignature.Payload payload = new()
        {
            Subject = "google-sub-456",
            Email = "testuser@gmail.com",
            GivenName = null,
            Name = null,
            FamilyName = null
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<GoogleJsonWebSignature.ValidationSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        Result<ExternalUserInfo> result = await provider.ValidateIdTokenAsync("valid-id-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("testuser");
        result.Value.LastName.Should().BeNull();
    }

    [Fact(DisplayName = "ValidateIdTokenAsync should return ExternalLoginEmailMissing when email is empty")]
    public async Task ValidateIdTokenAsync_WhenEmailMissing_ReturnsEmailMissingError()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();
        GoogleJsonWebSignature.Payload payload = new()
        {
            Subject = "google-sub-789",
            Email = string.Empty,
            GivenName = "No",
            FamilyName = "Email"
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<GoogleJsonWebSignature.ValidationSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        Result<ExternalUserInfo> result = await provider.ValidateIdTokenAsync("no-email-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "User.ExternalLogin.EmailMissing");
    }

    [Fact(DisplayName = "ValidateIdTokenAsync should return ExternalLoginEmailMissing when email is whitespace")]
    public async Task ValidateIdTokenAsync_WhenEmailWhitespace_ReturnsEmailMissingError()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();
        GoogleJsonWebSignature.Payload payload = new()
        {
            Subject = "google-sub-abc",
            Email = "   ",
            GivenName = "Only",
            FamilyName = "Spaces"
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<GoogleJsonWebSignature.ValidationSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        Result<ExternalUserInfo> result = await provider.ValidateIdTokenAsync("whitespace-email-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "User.ExternalLogin.EmailMissing");
    }

    [Fact(DisplayName = "ValidateIdTokenAsync should return ExternalLoginTokenInvalid when InvalidJwtException is thrown")]
    public async Task ValidateIdTokenAsync_WhenInvalidJwtException_ReturnsTokenInvalidError()
    {
        // Arrange
        GoogleExternalProvider provider = CreateProvider();

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<GoogleJsonWebSignature.ValidationSettings>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidJwtException("Token signature invalid"));

        // Act
        Result<ExternalUserInfo> result = await provider.ValidateIdTokenAsync("bad-token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "User.ExternalLogin.TokenInvalid");
    }
}
