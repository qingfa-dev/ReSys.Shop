using FluentValidation.TestHelper;

using Shared.Security.Authentication.Tokens.Options;

namespace Shared.UnitTests.Security.Authentication.Tokens.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "JwtSettings")]
public sealed class JwtSettingsValidatorTests
{
    private readonly JwtSettingsValidator _validator = new();

    private static JwtSettings CreateValidSettings()
    {
        return new JwtSettings
        {
            Secret = "the-krabby-patty-secret-formula-is-mine-32!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            Algorithm = "HS256",
            AccessTokenExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 7,
            TokenSecurity = new TokenSecurityOptions
            {
                RotationEnabled = true,
                ReuseDetectionEnabled = false,
                SlidingExpirationEnabled = true,
                MaxTokenAgeDays = 30
            }
        };
    }

    [Fact(DisplayName = "Valid settings should pass validation")]
    public void ValidSettings_ShouldPass()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty Secret should fail with SecretRequired error code")]
    public void EmptySecret_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Secret = string.Empty;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Secret)
            .WithErrorCode("Jwt.Secret.Required");
    }

    [Fact(DisplayName = "Secret shorter than 32 characters should fail with SecretTooShort error code")]
    public void SecretTooShort_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Secret = "short-secret";

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Secret)
            .WithErrorCode("Jwt.Secret.TooShort");
    }

    [Fact(DisplayName = "Empty Issuer should fail with IssuerRequired error code")]
    public void EmptyIssuer_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Issuer = string.Empty;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Issuer)
            .WithErrorCode("Jwt.Issuer.Required");
    }

    [Fact(DisplayName = "Empty Audience should fail with AudienceRequired error code")]
    public void EmptyAudience_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Audience = string.Empty;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Audience)
            .WithErrorCode("Jwt.Audience.Required");
    }

    [Fact(DisplayName = "Empty Algorithm should fail with AlgorithmRequired error code")]
    public void EmptyAlgorithm_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Algorithm = string.Empty;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Algorithm)
            .WithErrorCode("Jwt.Algorithm.Required");
    }

    [Fact(DisplayName = "Algorithm set to 'none' should fail with AlgorithmNoneNotAllowed error code")]
    public void AlgorithmNone_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Algorithm = "none";

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Algorithm)
            .WithErrorCode("Jwt.Algorithm.NoneNotAllowed");
    }

    [Fact(DisplayName = "Unsupported algorithm should fail with AlgorithmNotAllowed error code")]
    public void AlgorithmNotAllowed_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.Algorithm = "PS256";

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Algorithm)
            .WithErrorCode("Jwt.Algorithm.NotAllowed");
    }

    [Fact(DisplayName = "AccessTokenExpirationInMinutes set to 0 should fail with AccessTokenExpirationInvalid")]
    public void AccessTokenExpirationZero_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.AccessTokenExpirationInMinutes = 0;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessTokenExpirationInMinutes)
            .WithErrorCode("Jwt.AccessTokenExpiration.Invalid");
    }

    [Fact(DisplayName = "AccessTokenExpirationInMinutes exceeding 1440 should fail with AccessTokenExpirationExceeded")]
    public void AccessTokenExpirationExceeded_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.AccessTokenExpirationInMinutes = 9999;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessTokenExpirationInMinutes)
            .WithErrorCode("Jwt.AccessTokenExpiration.Exceeded");
    }

    [Fact(DisplayName = "RefreshTokenExpirationInDays set to 0 should fail with RefreshTokenExpirationInvalid")]
    public void RefreshTokenExpirationZero_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.RefreshTokenExpirationInDays = 0;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshTokenExpirationInDays)
            .WithErrorCode("Jwt.RefreshTokenExpiration.Invalid");
    }

    [Fact(DisplayName = "RefreshTokenExpirationInDays exceeding 365 should fail with RefreshTokenExpirationExceeded")]
    public void RefreshTokenExpirationExceeded_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.RefreshTokenExpirationInDays = 999;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshTokenExpirationInDays)
            .WithErrorCode("Jwt.RefreshTokenExpiration.Exceeded");
    }

    [Fact(DisplayName = "MaxTokenAgeDays set to 0 when ReuseDetectionEnabled should fail with MaxTokenAgeRequired")]
    public void MaxTokenAgeZero_WhenReuseDetectionEnabled_ShouldFail()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.TokenSecurity.ReuseDetectionEnabled = true;
        settings.TokenSecurity.MaxTokenAgeDays = 0;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TokenSecurity.MaxTokenAgeDays)
            .WithErrorCode("Jwt.TokenSecurity.MaxTokenAge.Required");
    }

    [Fact(DisplayName = "MaxTokenAgeDays set to 0 when ReuseDetectionDisabled should pass (conditional rule)")]
    public void MaxTokenAgeZero_WhenReuseDetectionDisabled_ShouldPass()
    {
        // Arrange
        JwtSettings settings = CreateValidSettings();
        settings.TokenSecurity.ReuseDetectionEnabled = false;
        settings.TokenSecurity.MaxTokenAgeDays = 0;

        // Act
        TestValidationResult<JwtSettings> result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TokenSecurity.MaxTokenAgeDays);
    }
}
