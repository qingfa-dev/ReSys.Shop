using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Refresh.Protections;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenBlacklist")]
public sealed class TokenBlacklistResultTests
{
    [Fact(DisplayName = "Success.Blacklisted should return a successful Result")]
    public void Success_Blacklisted_ShouldReturnSuccessResult()
    {
        Result result = TokenBlacklistResult.Success.Blacklisted;

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Success.NoReuseDetected should return Result<bool> with false")]
    public void Success_NoReuseDetected_ShouldReturnFalseValue()
    {
        Result<bool> result = TokenBlacklistResult.Success.NoReuseDetected;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "Failure.NotBlacklisted should return validation error with expected code")]
    public void Failure_NotBlacklisted_ShouldReturnExpectedError()
    {
        Error error = TokenBlacklistResult.Failure.NotBlacklisted;

        error.Code.Should().Be("TokenSecurity.Blacklisted");
        error.Message.Should().Be("Token is not blacklisted.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.BlacklistCheckFailed should return unexpected error with expected code")]
    public void Failure_BlacklistCheckFailed_ShouldReturnExpectedError()
    {
        Error error = TokenBlacklistResult.Failure.BlacklistCheckFailed;

        error.Code.Should().Be("TokenSecurity.BlacklistCheckFailed");
        error.Message.Should().Be("Failed to check token blacklist.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact(DisplayName = "Failure.BlacklistFailed should return unexpected error with expected code")]
    public void Failure_BlacklistFailed_ShouldReturnExpectedError()
    {
        Error error = TokenBlacklistResult.Failure.BlacklistFailed;

        error.Code.Should().Be("TokenSecurity.BlacklistFailed");
        error.Message.Should().Be("Failed to blacklist token.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact(DisplayName = "Failure.TheftDetectionFailed should return unexpected error with expected code")]
    public void Failure_TheftDetectionFailed_ShouldReturnExpectedError()
    {
        Error error = TokenBlacklistResult.Failure.TheftDetectionFailed;

        error.Code.Should().Be("TokenSecurity.TheftDetectionFailed");
        error.Message.Should().Be("Failed to detect token theft due to infrastructure Error.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact(DisplayName = "Failure.TheftDetectorError should return unexpected error with expected code")]
    public void Failure_TheftDetectorError_ShouldReturnExpectedError()
    {
        Error error = TokenBlacklistResult.Failure.TheftDetectorError;

        error.Code.Should().Be("TokenSecurity.TheftDetectorError");
        error.Message.Should().Be("Failed to detect token reuse due to infrastructure Error.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }
}
