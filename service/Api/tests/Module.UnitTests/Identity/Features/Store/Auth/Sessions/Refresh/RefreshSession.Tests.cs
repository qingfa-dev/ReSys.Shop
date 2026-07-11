using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Auth.Sessions.Refresh;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Sessions.Refresh;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Tokens")]
public class RefreshSessionTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IAccessTokenService> _accessTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;

    public RefreshSessionTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _accessTokenServiceMock = new Mock<IAccessTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
    }

    private RefreshSession.CommandHandler CreateHandler() => new(
        _userManagerMock.Object,
        _accessTokenServiceMock.Object,
        _refreshTokenServiceMock.Object);

    private static RefreshSession.Command CreateCommand(string refreshToken) => new(
        new RefreshSession.Request { RefreshToken = refreshToken });

    private static RefreshTokenResponseModel CreateRefreshResponse(Guid userId, string token = "new-refresh-token", DateTime? expiresAt = null) => new(
        Guid.NewGuid(),
        token,
        userId,
        DateTime.UtcNow,
        expiresAt ?? DateTime.UtcNow.AddDays(7),
        null, null, null, true);

    private void SetUpRotationSuccess(string oldToken, RefreshTokenResponseModel response) =>
        _refreshTokenServiceMock
            .Setup(x => x.RotateAsync(oldToken, TestContext.Current.CancellationToken))
            .ReturnsAsync(response);

    private void SetUpRotationFailure(string token) =>
        _refreshTokenServiceMock
            .Setup(x => x.RotateAsync(token, TestContext.Current.CancellationToken))
            .Returns(Task.FromResult<Result<RefreshTokenResponseModel>>(TokensResultHelper.Expired()));

    private void SetUpUserFound(User user) =>
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

    private void SetUpUserNotFound() =>
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    private void SetUpAccessTokenSuccess(string token = "new-access-token", long expiresIn = 3600) =>
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(new TokenResponseModel(token, expiresIn));

    private void SetUpAccessTokenFailure() =>
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(TokensResultHelper.GenerationFailed<TokenResponseModel>());

    // ==================== ROTATION FAILURE ====================

    [Fact(DisplayName = "Should return failure when refresh token rotation fails")]
    public async Task Handle_ShouldReturnFailure_WhenRotationFails()
    {
        SetUpRotationFailure("invalid-token");

        var result = await CreateHandler().Handle(CreateCommand("invalid-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("RefreshToken.Expired");
    }

    // ==================== USER NOT FOUND ====================

    [Fact(DisplayName = "Should return NotFound when user associated with token is not found")]
    public async Task Handle_ShouldReturnNotFound_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        SetUpRotationSuccess("valid-token", CreateRefreshResponse(userId));
        SetUpUserNotFound();

        var result = await CreateHandler().Handle(CreateCommand("valid-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    // ==================== INACTIVE USER ====================

    [Fact(DisplayName = "Should return Inactive when user account is deactivated")]
    public async Task Handle_ShouldReturnInactive_WhenUserIsInactive()
    {
        var userId = Guid.NewGuid();
        User user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.IsActive = false;

        SetUpRotationSuccess("valid-token", CreateRefreshResponse(userId));
        SetUpUserFound(user);

        var result = await CreateHandler().Handle(CreateCommand("valid-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.Inactive.Code);
    }

    // ==================== ACCESS TOKEN FAILURE ====================

    [Fact(DisplayName = "Should return failure when access token generation fails")]
    public async Task Handle_ShouldReturnFailure_WhenAccessTokenGenerationFails()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        SetUpRotationSuccess("valid-token", CreateRefreshResponse(userId));
        SetUpUserFound(user);
        SetUpAccessTokenFailure();

        var result = await CreateHandler().Handle(CreateCommand("valid-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== SUCCESSFUL REFRESH ====================

    [Fact(DisplayName = "Should return new tokens when refresh token is valid")]
    public async Task Handle_ShouldReturnTokens_WhenRefreshTokenIsValid()
    {
        const string oldToken = "old-refresh-token";
        const string newRefreshToken = "new-refresh-token";
        const string accessToken = "new-access-token";
        const long expiresIn = 3600;

        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        SetUpRotationSuccess(oldToken, CreateRefreshResponse(userId, newRefreshToken));
        SetUpUserFound(user);
        SetUpAccessTokenSuccess(accessToken, expiresIn);

        var result = await CreateHandler().Handle(CreateCommand(oldToken), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.AccessTokenExpiresIn.Should().Be(expiresIn);
        result.Value.RefreshToken.Should().Be(newRefreshToken);
    }

    [Fact(DisplayName = "Should pass correct user data in TokenRequest")]
    public async Task Handle_ShouldPassCorrectUserDataInTokenRequestModel()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.Id = userId;

        TokenRequestModel? captured = null;
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Callback<TokenRequestModel>(r => captured = r)
            .Returns(new TokenResponseModel("access-token", 3600));

        SetUpRotationSuccess("valid-token", CreateRefreshResponse(userId));
        SetUpUserFound(user);

        await CreateHandler().Handle(CreateCommand("valid-token"), TestContext.Current.CancellationToken);

        captured.Should().NotBeNull();
        captured.UserId.Should().Be(user.Id);
        captured.Email.Should().Be(user.Email);
        captured.FullName.Should().Be(user.FullName);
    }

    [Fact(DisplayName = "Should map refresh token expiry to Unix timestamp in response")]
    public async Task Handle_ShouldMapRefreshTokenExpiryToUnixTimestamp()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        SetUpRotationSuccess("valid-token", CreateRefreshResponse(userId, "new-token", expiresAt));
        SetUpUserFound(user);
        SetUpAccessTokenSuccess();

        var result = await CreateHandler().Handle(CreateCommand("valid-token"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshTokenExpiresIn.Should().Be(new DateTimeOffset(expiresAt).ToUnixTimeSeconds());
    }
}

internal static class TokensResultHelper
{
    public static Error Expired() => Error.Unexpected("RefreshToken.Expired", "Refresh token has expired.");
    public static Result<T> GenerationFailed<T>() => Error.Unexpected("AccessToken.GenerationFailed", "Failed to generate access token.");
}
