using Microsoft.AspNetCore.Identity;

using static Module.Identity.Features.Store.Auth.Logout.Logout;

using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Modules.Tests.Identities.Features.Store.Auth.Logout;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Logouts")]
public class LogoutTests
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ILogger<Command>> _loggerMock;

    public LogoutTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _loggerMock = new Mock<ILogger<Command>>();
    }

    private CommandHandler CreateHandler() => new(
        _currentUserMock.Object,
        _refreshTokenServiceMock.Object,
        _userManagerMock.Object,
        _loggerMock.Object);

    private static Command CreateCommand(string? refreshToken = null, bool revokeAll = false) => new(
        new Request { RefreshToken = refreshToken, RevokeAll = revokeAll });

    private void SetUpAuthenticated(Guid userId)
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId.ToString());
    }

    private void SetUpUnauthenticated()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);
    }

    private void SetUpInvalidUserId()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns("not-a-guid");
    }

    private void SetUpUserFound(User user, Guid userId) =>
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

    private void SetUpUserNotFound(Guid userId) =>
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

    private void SetUpRevokeSuccess() =>
        _refreshTokenServiceMock
            .Setup(x => x.RevokeAsync(It.IsAny<RevokeTokenRequestModel>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Ok());

    private void SetUpRevokeFailure() =>
        _refreshTokenServiceMock
            .Setup(x => x.RevokeAsync(It.IsAny<RevokeTokenRequestModel>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Error.Unexpected("RevokeFailed", "Failed to revoke"));

    private void SetUpRevokeAllSuccess(int count = 0) =>
        _refreshTokenServiceMock
            .Setup(x => x.RevokeAllForUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(count);

    private void SetUpRevokeAllFailure() =>
        _refreshTokenServiceMock
            .Setup(x => x.RevokeAllForUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), TestContext.Current.CancellationToken))
            .Returns(Task.FromResult<Result<int>>(Error.Unexpected("RevokeAllFailed", "Failed to revoke all")));

    // ==================== AUTHENTICATION CHECKS ====================

    [Fact(DisplayName = "Should return InvalidCredentials when user is not authenticated")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenUserNotAuthenticated()
    {
        SetUpUnauthenticated();

        var result = await CreateHandler().Handle(CreateCommand("some-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    [Fact(DisplayName = "Should return InvalidCredentials when user id is not a valid GUID")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenUserIdInvalid()
    {
        SetUpInvalidUserId();

        var result = await CreateHandler().Handle(CreateCommand("some-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    // ==================== USER NOT FOUND ====================

    [Fact(DisplayName = "Should return NotFound when user is not found in database")]
    public async Task Handle_ShouldReturnNotFound_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        SetUpAuthenticated(userId);
        SetUpUserNotFound(userId);

        var result = await CreateHandler().Handle(CreateCommand("some-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    // ==================== SINGLE TOKEN REVOCATION ====================

    [Fact(DisplayName = "Should return Accepted when single token revoked")]
    public async Task Handle_ShouldReturnAccepted_WhenSingleTokenRevoked()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(userId);
        SetUpUserFound(user, userId);
        SetUpRevokeSuccess();

        var result = await CreateHandler().Handle(CreateCommand("refresh-token-value"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return failure when single token revoke fails")]
    public async Task Handle_ShouldReturnFailure_WhenSingleTokenRevokeFails()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(userId);
        SetUpUserFound(user, userId);
        SetUpRevokeFailure();

        var result = await CreateHandler().Handle(CreateCommand("refresh-token-value"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== REVOKE ALL ====================

    [Fact(DisplayName = "Should return Ok when all tokens revoked")]
    public async Task Handle_ShouldReturnOk_WhenAllTokensRevoked()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(userId);
        SetUpUserFound(user, userId);
        SetUpRevokeAllSuccess(3);

        var result = await CreateHandler().Handle(CreateCommand(revokeAll: true), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return failure when revoke all fails")]
    public async Task Handle_ShouldReturnFailure_WhenRevokeAllFails()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(userId);
        SetUpUserFound(user, userId);
        SetUpRevokeAllFailure();

        var result = await CreateHandler().Handle(CreateCommand(revokeAll: true), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== NO TOKEN (SOFT LOGOUT) ====================

    [Fact(DisplayName = "Should return Accepted when no refresh token provided")]
    public async Task Handle_ShouldReturnAccepted_WhenNoRefreshToken()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(userId);
        SetUpUserFound(user, userId);

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _refreshTokenServiceMock.Verify(
            x => x.RevokeAsync(It.IsAny<RevokeTokenRequestModel>(), TestContext.Current.CancellationToken),
            Times.Never);
    }
}
