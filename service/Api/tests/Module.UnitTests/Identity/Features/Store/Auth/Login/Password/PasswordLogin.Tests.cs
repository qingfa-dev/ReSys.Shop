using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Auth.Login.Password;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Login.Password;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Logins")]
public class PasswordLoginTests
{
    private readonly Mock<ISystemDateTime> _systemDatetime;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IAccessTokenService> _accessTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<PasswordLogin.CommandHandler>> _loggerMock;

    public PasswordLoginTests()
    {
        _systemDatetime = new Mock<ISystemDateTime>();
        _systemDatetime.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);
        _systemDatetime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _signInManagerMock = IdentityMocks.CreateSignInManagerMock(_userManagerMock);
        _accessTokenServiceMock = new Mock<IAccessTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _loggerMock = new Mock<ILogger<PasswordLogin.CommandHandler>>();

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.IpAddress).Returns("192.168.1.1");
        _currentUserMock.Setup(x => x.Device).Returns("Chrome/120");
    }

    // ===== Factory =====

    private PasswordLogin.CommandHandler CreateHandler() => new(
        _systemDatetime.Object,
        _signInManagerMock.Object,
        _userManagerMock.Object,
        _accessTokenServiceMock.Object,
        _refreshTokenServiceMock.Object,
        _currentUserMock.Object,
        _loggerMock.Object);

    private static PasswordLogin.Command CreateCommand(string credential, string password) => new(
        new PasswordLogin.Request { Credential = credential, Password = password });

    private static RefreshTokenResponseModel CreateRefreshToken(Guid userId, DateTime? expiresAt = null) => new(
        Guid.NewGuid(),
        "refresh-token",
        userId,
        DateTime.UtcNow,
        expiresAt ?? DateTime.UtcNow.AddDays(7),
        null, null, null, true);

    // ===== Mock Setup Helpers =====

    private void SetUpUsersQueryable(params User[] users) =>
        _userManagerMock.Setup(x => x.Users).Returns(users.AsQueryable());

    private void SetUpSignInSuccess() =>
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

    private void SetUpSignInFailure(SignInResult result) =>
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(result);

    private void SetUpAccessTokenSuccess(string token = "access-token", long expiresIn = 3600) =>
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Result<TokenResponseModel>.Ok(new TokenResponseModel(token, expiresIn)));

    private void SetUpAccessTokenFailure() =>
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Error.Unexpected("Token.Generation.Failed", "Failed to generate token"));

    private void SetUpRefreshTokenSuccess(Guid userId) =>
        _refreshTokenServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<RefreshTokenResponseModel>.Ok(CreateRefreshToken(userId)));

    private void SetUpRefreshTokenFailure() =>
        _refreshTokenServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Error.Unexpected("RefreshToken.Generation.Failed", "Failed to generate refresh token"));

    private void SetUpUpdateSuccess() =>
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

    private void SetUpUpdateFailure() =>
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Failed to update." }));

    private void SetUpSuccessScenario(User user)
    {
        SetUpUsersQueryable(user);
        SetUpSignInSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();
    }

    // ==================== USER NOT FOUND ====================

    [Fact(DisplayName = "Should return InvalidCredentials when user not found")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenUserNotFound()
    {
        SetUpUsersQueryable();

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    // ==================== SIGN-IN FAILURES ====================

    [Fact(DisplayName = "Should return InvalidCredentials when password is wrong")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenPasswordIsWrong()
    {
        SetUpUsersQueryable(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);
        SetUpSignInFailure(SignInResult.Failed);

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "wrongpassword"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    [Fact(DisplayName = "Should return InvalidCredentials when account is locked out")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenAccountLockedOut()
    {
        SetUpUsersQueryable(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);
        SetUpSignInFailure(SignInResult.LockedOut);

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    [Fact(DisplayName = "Should return InvalidCredentials when sign in is not allowed")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenSignInNotAllowed()
    {
        SetUpUsersQueryable(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);
        SetUpSignInFailure(SignInResult.NotAllowed);

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    // ==================== INACTIVE USER ====================

    [Fact(DisplayName = "Should return Inactive when user account is deactivated")]
    public async Task Handle_ShouldReturnInactive_WhenUserIsNotActive()
    {
        User inactiveUser = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        inactiveUser.IsActive = false;
        SetUpUsersQueryable(inactiveUser);
        SetUpSignInSuccess();

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.Inactive.Code);
    }

    // ==================== TOKEN GENERATION FAILURES ====================

    [Fact(DisplayName = "Should return failure when access token generation fails")]
    public async Task Handle_ShouldReturnFailure_WhenAccessTokenGenerationFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpUsersQueryable(user);
        SetUpSignInSuccess();
        SetUpAccessTokenFailure();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return failure when refresh token generation fails")]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenGenerationFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpUsersQueryable(user);
        SetUpSignInSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenFailure();

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== UPDATE FAILURES ====================

    [Fact(DisplayName = "Should return failure when UpdateAsync fails after token generation")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpUsersQueryable(user);
        SetUpSignInSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateFailure();

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Should not call UpdateAsync when login fails")]
    public async Task Handle_ShouldNotCallUpdate_WhenLoginFails()
    {
        SetUpUsersQueryable(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);
        SetUpSignInFailure(SignInResult.Failed);

        await CreateHandler().Handle(CreateCommand("test@example.com", "wrongpassword"), TestContext.Current.CancellationToken);

        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // ==================== SUCCESSFUL LOGIN ====================

    [Fact(DisplayName = "Should return access and refresh tokens on successful login")]
    public async Task Handle_ShouldReturnTokens_OnSuccessfulLogin()
    {
        const long expiresIn = 7200;
        var refreshExp = DateTime.UtcNow.AddDays(14);
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpUsersQueryable(user);
        SetUpSignInSuccess();
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Result<TokenResponseModel>.Ok(new TokenResponseModel("jwt-token-abc123", expiresIn)));
        _refreshTokenServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<RefreshTokenResponseModel>.Ok(CreateRefreshToken(user.Id, refreshExp) with
            {
                Token = "refresh-token-xyz"
            }));
        SetUpUpdateSuccess();

        var result = await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-token-abc123");
        result.Value.AccessTokenExpiresIn.Should().Be(expiresIn);
        result.Value.RefreshToken.Should().Be("refresh-token-xyz");
        result.Value.RefreshTokenExpiresIn.Should().Be(new DateTimeOffset(refreshExp).ToUnixTimeSeconds());
    }

    [Fact(DisplayName = "Should create TokenRequest with correct user data")]
    public async Task Handle_ShouldCreateTokenRequest_WithUserData()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.FirstName = "Jane";
        user.LastName = "Smith";

        SetUpUsersQueryable(user);
        SetUpSignInSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();

        TokenRequestModel? captured = null;
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Callback<TokenRequestModel>(r => captured = r)
            .Returns(Result<TokenResponseModel>.Ok(new TokenResponseModel("token", 3600)));

        await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        captured.Should().NotBeNull();
        captured.UserId.Should().Be(user.Id);
        captured.Email.Should().Be(user.Email);
        captured.FullName.Should().Be(user.FullName);
    }

    // ==================== LOCKOUT PARAMETER ====================

    [Fact(DisplayName = "Should pass lockoutOnFailure true to CheckPasswordSignInAsync")]
    public async Task Handle_ShouldPassLockoutOnFailure_True()
    {
        bool? capturedLockout = null;
        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Callback<User, string, bool>((_, _, lockout) => capturedLockout = lockout)
            .ReturnsAsync(SignInResult.Success);

        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpUsersQueryable(user);
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();

        await CreateHandler().Handle(CreateCommand("test@example.com", "password123"), TestContext.Current.CancellationToken);

        capturedLockout.Should().BeTrue();
    }
}
