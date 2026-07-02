using Shared.Security.Authentication.Tokens.Options;

namespace Shared.UnitTests.Security.Authentication.Tokens.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "JwtSettings")]
public sealed class JwtSettingsResultTests
{
    [Fact(DisplayName = "Success.Valid should return expected message")]
    public void Success_Valid_ShouldReturnExpectedMessage()
    {
        JwtSettingsResult.Success.Valid.Should().Be("JWT settings are valid.");
    }

    [Fact(DisplayName = "Success.WeakSecretWarning should return expected message")]
    public void Success_WeakSecretWarning_ShouldReturnExpectedMessage()
    {
        JwtSettingsResult.Success.WeakSecretWarning.Should().Be("JWT settings are valid but use a weak or default secret.");
    }

    [Fact(DisplayName = "Failure.SettingsNull should return validation error with expected code")]
    public void Failure_SettingsNull_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.SettingsNull;

        error.Code.Should().Be("Jwt.Settings.Null");
        error.Message.Should().Be("JWT settings configuration is null.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.SecretRequired should return validation error with expected code")]
    public void Failure_SecretRequired_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.SecretRequired;

        error.Code.Should().Be("Jwt.Secret.Required");
        error.Message.Should().Be("JWT secret is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.SecretTooShort should return validation error with MinLength in message")]
    public void Failure_SecretTooShort_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.SecretTooShort;

        error.Code.Should().Be("Jwt.Secret.TooShort");
        error.Message.Should().Contain("32");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.IssuerRequired should return validation error with expected code")]
    public void Failure_IssuerRequired_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.IssuerRequired;

        error.Code.Should().Be("Jwt.Issuer.Required");
        error.Message.Should().Be("JWT issuer is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.AudienceRequired should return validation error with expected code")]
    public void Failure_AudienceRequired_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.AudienceRequired;

        error.Code.Should().Be("Jwt.Audience.Required");
        error.Message.Should().Be("JWT audience is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.AlgorithmRequired should return validation error with expected code")]
    public void Failure_AlgorithmRequired_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.AlgorithmRequired;

        error.Code.Should().Be("Jwt.Algorithm.Required");
        error.Message.Should().Be("JWT algorithm is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.AlgorithmNoneNotAllowed should return validation error with expected code")]
    public void Failure_AlgorithmNoneNotAllowed_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.AlgorithmNoneNotAllowed;

        error.Code.Should().Be("Jwt.Algorithm.NoneNotAllowed");
        error.Message.Should().Contain("none");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.AlgorithmNotAllowed should return validation error with allowed algorithms in message")]
    public void Failure_AlgorithmNotAllowed_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.AlgorithmNotAllowed;

        error.Code.Should().Be("Jwt.Algorithm.NotAllowed");
        error.Message.Should().Contain("HS256");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.AccessTokenExpirationInvalid should return validation error with expected code")]
    public void Failure_AccessTokenExpirationInvalid_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.AccessTokenExpirationInvalid;

        error.Code.Should().Be("Jwt.AccessTokenExpiration.Invalid");
        error.Message.Should().Be("Access token expiration must be greater than 0 minutes.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.AccessTokenExpirationExceeded should return validation error with MaxMinutes in message")]
    public void Failure_AccessTokenExpirationExceeded_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.AccessTokenExpirationExceeded;

        error.Code.Should().Be("Jwt.AccessTokenExpiration.Exceeded");
        error.Message.Should().Contain("1440");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.RefreshTokenExpirationInvalid should return validation error with expected code")]
    public void Failure_RefreshTokenExpirationInvalid_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.RefreshTokenExpirationInvalid;

        error.Code.Should().Be("Jwt.RefreshTokenExpiration.Invalid");
        error.Message.Should().Be("Refresh token expiration must be greater than 0 days.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.RefreshTokenExpirationExceeded should return validation error with MaxDays in message")]
    public void Failure_RefreshTokenExpirationExceeded_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.RefreshTokenExpirationExceeded;

        error.Code.Should().Be("Jwt.RefreshTokenExpiration.Exceeded");
        error.Message.Should().Contain("365");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.MaxTokenAgeRequired should return validation error with expected code")]
    public void Failure_MaxTokenAgeRequired_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.MaxTokenAgeRequired;

        error.Code.Should().Be("Jwt.TokenSecurity.MaxTokenAge.Required");
        error.Message.Should().Be("Max token age must be greater than 0 days when reuse detection is enabled.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.WeakSecretWarning should return validation error with expected code")]
    public void Failure_WeakSecretWarning_ShouldReturnExpectedError()
    {
        Error error = JwtSettingsResult.Failure.WeakSecretWarning;

        error.Code.Should().Be("Jwt.Secret.Weak");
        error.Message.Should().Be("JWT secret appears to be weak or a default value. This should be changed for production environments.");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
