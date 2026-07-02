using Shared.Security.Authentication.Tokens.Services.Access;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Access;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "AccessTokenService")]
public sealed class AccessTokenResultTests
{
    [Fact(DisplayName = "Success.Generated should return a successful Result")]
    public void Success_Generated_ShouldReturnSuccessResult()
    {
        Result result = AccessTokenResult.Success.Generated;

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Failure.InvalidConfiguration should return unexpected error with expected code")]
    public void Failure_InvalidConfiguration_ShouldReturnExpectedError()
    {
        Error error = AccessTokenResult.Failure.InvalidConfiguration;

        error.Code.Should().Be("AccessToken.InvalidConfiguration");
        error.Message.Should().Be("Invalid JWT configuration.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact(DisplayName = "Failure.GenerationFailed should return unexpected error with expected code")]
    public void Failure_GenerationFailed_ShouldReturnExpectedError()
    {
        Error error = AccessTokenResult.Failure.GenerationFailed;

        error.Code.Should().Be("AccessToken.GenerationFailed");
        error.Message.Should().Be("Failed to generate access token.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact(DisplayName = "Failure.TokenValidationFailed should return unexpected error with expected code")]
    public void Failure_TokenValidationFailed_ShouldReturnExpectedError()
    {
        Error error = AccessTokenResult.Failure.TokenValidationFailed;

        error.Code.Should().Be("AccessToken.TokenValidationFailed");
        error.Message.Should().Be("Failed to validate the generated access token.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }
}
