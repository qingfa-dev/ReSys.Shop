using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Auth.Login.External.Authenticate;
using Module.Profile.Features.Store.Profiles.Create;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;
using Shared.Security.Identity.Domain.Users.Logins;

namespace Module.UnitTests.Identity.Features.Store.Auth.Login.External.Authenticate;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Logins")]
public class ExternalAuthenticateTests
{
    private readonly Mock<IExternalLoginProvider> _providerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IAccessTokenService> _accessTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ISystemDateTime> _dateTimeMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<ExternalAuthenticate.CommandHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DateTimeOffset _fixedNow;

    public ExternalAuthenticateTests()
    {
        _providerMock = new Mock<IExternalLoginProvider>();
        _providerMock.Setup(p => p.Provider).Returns("google");

        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _accessTokenServiceMock = new Mock<IAccessTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        _fixedNow = DateTimeOffset.UtcNow;
        _dateTimeMock = new Mock<ISystemDateTime>();
        _dateTimeMock.Setup(x => x.UtcNow).Returns(_fixedNow);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.IpAddress).Returns("192.168.1.1");
        _currentUserMock.Setup(x => x.Device).Returns("Chrome/120");

        _loggerMock = new Mock<ILogger<ExternalAuthenticate.CommandHandler>>();
        _mediatorMock = new Mock<IMediator>();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateProfile.Response());
    }

    // ===== Factory =====

    private ExternalAuthenticate.CommandHandler CreateHandler() => new(
        [_providerMock.Object],
        _userManagerMock.Object,
        _accessTokenServiceMock.Object,
        _refreshTokenServiceMock.Object,
        _dateTimeMock.Object,
        _currentUserMock.Object,
        _loggerMock.Object,
        _mediatorMock.Object);

    private static ExternalAuthenticate.Command CreateCommand(string provider, string idToken) => new(
        new ExternalAuthenticate.Request { Provider = provider, IdToken = idToken });

    private static RefreshTokenResponseModel CreateRefreshToken(Guid userId) => new(
        Guid.NewGuid(),
        "refresh-token",
        userId,
        DateTime.UtcNow,
        DateTime.UtcNow.AddDays(7),
        null, null, null, true);

    // ===== Mock Setup Helpers =====

    private void SetUpProviderSuccess(string subjectId = "google-subject-123", string email = "user@gmail.com")
    {
        _providerMock
            .Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<ExternalUserInfo>.Ok(new ExternalUserInfo(
                Provider: "google",
                ProviderSubjectId: subjectId,
                Email: email,
                FirstName: "John",
                LastName: "Doe")));
    }

    private void SetUpProviderFailure()
    {
        _providerMock
            .Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(UserResult.Failure.InvalidCredentials);
    }

    private void SetUpCreateUserSuccess()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetUpCreateUserFailure()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "CreateFailed", Description = "Failed to create user."
            }));
    }

    private void SetUpAddRoleSuccess()
    {
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetUpAddRoleFailure()
    {
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "RoleFailed", Description = "Failed to add role."
            }));
    }

    private void SetUpAddLoginSuccess()
    {
        _userManagerMock
            .Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private void SetUpAddLoginFailure()
    {
        _userManagerMock
            .Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "LoginFailed", Description = "Failed to add login."
            }));
    }

    private void SetUpAccessTokenSuccess(string token = "access-token", long expiresIn = 3600)
    {
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Result<TokenResponseModel>.Ok(new TokenResponseModel(token, expiresIn)));
    }

    private void SetUpAccessTokenFailure()
    {
        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Error.Unexpected("Token.Generation.Failed", "Failed to generate token"));
    }

    private void SetUpRefreshTokenSuccess(Guid userId)
    {
        _refreshTokenServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<RefreshTokenResponseModel>.Ok(CreateRefreshToken(userId)));
    }

    private void SetUpRefreshTokenFailure()
    {
        _refreshTokenServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Error.Unexpected("RefreshToken.Generation.Failed", "Failed to generate refresh token"));
    }

    private void SetUpUpdateSuccess()
    {
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
    }

    private void SetUpUpdateFailure()
    {
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateFailed", Description = "Failed to update."
            }));
    }

    private void SetUpNewUserScenario(Guid userId)
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess();
        SetUpCreateUserSuccess();
        SetUpAddRoleSuccess();
        SetUpAddLoginSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(userId);
        SetUpUpdateSuccess();
    }

    // ==================== UNSUPPORTED PROVIDER ====================

    [Fact(DisplayName = "Should return ExternalLoginUnsupportedProvider when provider not found")]
    public async Task Handle_UnsupportedProvider_ReturnsUnsupportedProvider()
    {
        var result = await CreateHandler().Handle(
            CreateCommand("unsupported", "some-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.ExternalLoginUnsupportedProvider.Code);
    }

    // ==================== TOKEN VALIDATION FAILURE ====================

    [Fact(DisplayName = "Should return InvalidCredentials when token validation fails")]
    public async Task Handle_TokenValidationFails_ReturnsInvalidCredentials()
    {
        SetUpProviderFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "bad-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    // ==================== NEW USER CREATION ====================

    [Fact(DisplayName = "Should create new user when email not found")]
    public async Task Handle_NewUser_CreatesUserWithCorrectProperties()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess();

        User? capturedUser = null;
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync(IdentityResult.Success);

        SetUpAddRoleSuccess();
        SetUpAddLoginSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(userId);
        SetUpUpdateSuccess();

        await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("user@gmail.com");
        capturedUser.FirstName.Should().Be("John");
        capturedUser.LastName.Should().Be("Doe");
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.EmailConfirmed.Should().BeTrue();
        capturedUser.CreatedAtUtc.Should().Be(_fixedNow);
        capturedUser.UserName.Should().NotBeNullOrEmpty();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Should assign default User role when creating new user")]
    public async Task Handle_NewUser_AssignsDefaultRole()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess();
        SetUpCreateUserSuccess();
        SetUpAddLoginSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(userId);
        SetUpUpdateSuccess();

        string? capturedRole = null;
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((_, role) => capturedRole = role)
            .ReturnsAsync(IdentityResult.Success);

        await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        capturedRole.Should().Be(RoleConstant.Defaults.User);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Should link external login when creating new user")]
    public async Task Handle_NewUser_LinksExternalLogin()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess(subjectId: "sub-456");
        SetUpCreateUserSuccess();
        SetUpAddRoleSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(userId);
        SetUpUpdateSuccess();

        UserLoginInfo? capturedLogin = null;
        _userManagerMock
            .Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()))
            .Callback<User, UserLoginInfo>((_, login) => capturedLogin = login)
            .ReturnsAsync(IdentityResult.Success);

        await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        capturedLogin.Should().NotBeNull();
        capturedLogin!.LoginProvider.Should().Be("google");
        capturedLogin.ProviderKey.Should().Be("sub-456");
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Should create profile with correct user data for new external user")]
    public async Task Handle_NewUser_CreatesProfileWithCorrectData()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess(subjectId: "sub-789", email: "external@test.com");
        SetUpCreateUserSuccess();
        SetUpAddRoleSuccess();
        SetUpAddLoginSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(userId);
        SetUpUpdateSuccess();

        CreateProfile.Command? captured = null;
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<CreateProfile.Response>>, CancellationToken>((req, _) => captured = (CreateProfile.Command)req)
            .ReturnsAsync(new CreateProfile.Response());

        await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        captured.Should().NotBeNull();
        captured.UserId.Should().NotBeEmpty();
        captured.Request.FirstName.Should().Be("John");
        captured.Request.LastName.Should().Be("Doe");
        captured.Request.Email.Should().Be("external@test.com");
    }

    // ==================== EXISTING USER — LINK NEW PROVIDER ====================

    [Fact(DisplayName = "Should link new provider when user exists without matching login")]
    public async Task Handle_ExistingUserNoLogin_LinksNewProvider()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.UserLogins = [];

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess(subjectId: "new-provider-sub");

        UserLoginInfo? capturedLogin = null;
        _userManagerMock
            .Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()))
            .Callback<User, UserLoginInfo>((_, login) => capturedLogin = login)
            .ReturnsAsync(IdentityResult.Success);

        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();

        await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        capturedLogin.Should().NotBeNull();
        capturedLogin!.LoginProvider.Should().Be("google");
        capturedLogin.ProviderKey.Should().Be("new-provider-sub");
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ==================== EXISTING USER — LOGIN ALREADY LINKED ====================

    [Fact(DisplayName = "Should skip AddLoginAsync when existing login already linked")]
    public async Task Handle_ExistingUserWithLogin_SkipsAddLogin()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.UserLogins =
        [
            new UserLogin { LoginProvider = "google", ProviderKey = "existing-sub" }
        ];

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess(subjectId: "existing-sub");
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();

        await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        _userManagerMock.Verify(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()), Times.Never);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ==================== CREATION FAILURES ====================

    [Fact(DisplayName = "Should return failure when CreateAsync fails")]
    public async Task Handle_CreateUserFails_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess();
        SetUpCreateUserFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Should return failure when AddToRoleAsync fails")]
    public async Task Handle_AddRoleFails_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess();
        SetUpCreateUserSuccess();
        SetUpAddRoleFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Should return failure when AddLoginAsync fails for new user")]
    public async Task Handle_AddLoginFails_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        SetUpProviderSuccess();
        SetUpCreateUserSuccess();
        SetUpAddRoleSuccess();
        SetUpAddLoginFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ==================== INACTIVE USER ====================

    [Fact(DisplayName = "Should return Inactive when user account is deactivated")]
    public async Task Handle_InactiveUser_ReturnsInactive()
    {
        User user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.IsActive = false;
        user.UserLogins = [new UserLogin { LoginProvider = "google", ProviderKey = "google-subject-123" }];
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateSuccess();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.Inactive.Code);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ==================== TOKEN GENERATION FAILURES ====================

    [Fact(DisplayName = "Should return failure when access token generation fails")]
    public async Task Handle_AccessTokenFails_ReturnsFailure()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.UserLogins = [new UserLogin { LoginProvider = "google", ProviderKey = "google-subject-123" }];
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess();
        SetUpAccessTokenFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Should return failure when refresh token generation fails")]
    public async Task Handle_RefreshTokenFails_ReturnsFailure()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.UserLogins = [new UserLogin { LoginProvider = "google", ProviderKey = "google-subject-123" }];
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ==================== UPDATE FAILURES ====================

    [Fact(DisplayName = "Should return failure when UpdateAsync fails after token generation")]
    public async Task Handle_UpdateFails_ReturnsFailure()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.UserLogins = [new UserLogin { LoginProvider = "google", ProviderKey = "google-subject-123" }];
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess();
        SetUpAccessTokenSuccess();
        SetUpRefreshTokenSuccess(user.Id);
        SetUpUpdateFailure();

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ==================== SUCCESSFUL LOGIN ====================

    [Fact(DisplayName = "Should return tokens on successful external login")]
    public async Task Handle_Success_ReturnsTokens()
    {
        const long expiresIn = 7200;
        var refreshExp = DateTime.UtcNow.AddDays(14);
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.UserLogins = [new UserLogin { LoginProvider = "google", ProviderKey = "google-subject-123" }];
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        SetUpProviderSuccess();

        _accessTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Result<TokenResponseModel>.Ok(new TokenResponseModel("jwt-token-abc123", expiresIn)));

        _refreshTokenServiceMock
            .Setup(x => x.GenerateAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<RefreshTokenResponseModel>.Ok(CreateRefreshToken(user.Id) with
            {
                Token = "refresh-token-xyz", ExpiresAt = refreshExp
            }));

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await CreateHandler().Handle(
            CreateCommand("google", "valid-token"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-token-abc123");
        result.Value.AccessTokenExpiresIn.Should().Be(expiresIn);
        result.Value.RefreshToken.Should().Be("refresh-token-xyz");
        result.Value.RefreshTokenExpiresIn.Should().Be(new DateTimeOffset(refreshExp).ToUnixTimeSeconds());
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}