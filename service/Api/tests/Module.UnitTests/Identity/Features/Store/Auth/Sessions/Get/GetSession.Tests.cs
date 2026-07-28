using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Auth.Sessions.Get;
using Module.Profile.Domain;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Sessions.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Session")]
public class GetSessionTests
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<UserManager<User>> _userManagerMock;

    public GetSessionTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
    }

    private GetSession.QueryHandler CreateHandler() => new(
        _currentUserMock.Object,
        _permissionServiceMock.Object,
        _userManagerMock.Object);

    private void SetUpAuthenticated(Guid userId)
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId.ToString());
    }

    private void SetUpUnauthenticated() =>
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

    private void SetUpUserFound(User user) =>
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

    private void SetUpUserNotFound() =>
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    private void SetUpRoles(string[] roles) =>
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(roles);

    private void SetUpPermissions(HashSet<string> permissions) =>
        _permissionServiceMock
            .Setup(x => x.GetEffectiveUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

    private void SetUpPermissionsFailure() =>
        _permissionServiceMock
            .Setup(x => x.GetEffectiveUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unexpected("perm.fail", "Failed to get permissions"));

    // ==================== AUTHENTICATION ====================

    [Fact(DisplayName = "Should return Unauthorized when user not authenticated")]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        SetUpUnauthenticated();

        var result = await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.AuthRequired.Code);
    }

    // ==================== USER NOT FOUND ====================

    [Fact(DisplayName = "Should return NotFound when user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserNotFound()
    {
        SetUpAuthenticated(Guid.NewGuid());
        SetUpUserNotFound();

        var result = await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return NotFound when user ID is null")]
    public async Task Handle_ShouldReturnNotFound_WhenUserIdIsNull()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);
        SetUpUserNotFound();

        var result = await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    // ==================== SUCCESSFUL SESSION ====================

    [Fact(DisplayName = "Should return session with roles and permissions when user exists")]
    public async Task Handle_ShouldReturnSession_WhenUserExists()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var roles = new[] { "Admin", "User" };
        var permissions = new HashSet<string> { "read:products", "write:products" };

        SetUpAuthenticated(user.Id);
        SetUpUserFound(user);
        SetUpRoles(roles);
        SetUpPermissions(permissions);

        var result = await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be(user.Email);
        result.Value.UserName.Should().Be(user.UserName);
        result.Value.Roles.Should().BeEquivalentTo(roles);
        result.Value.Permissions.Should().BeEquivalentTo(permissions);
    }

    [Fact(DisplayName = "Should return empty roles when user has no roles")]
    public async Task Handle_ShouldReturnEmptyRoles_WhenUserHasNoRoles()
    {
        SetUpAuthenticated(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value.Id);
        SetUpUserFound(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);
        SetUpRoles([]);
        SetUpPermissions([]);

        var result = await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Roles.Should().BeEmpty();
        result.Value.Permissions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should handle permission store failure gracefully")]
    public async Task Handle_ShouldHandlePermissionStoreFailure_WhenPermissionsFail()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var roles = new[] { "User" };

        SetUpAuthenticated(user.Id);
        SetUpUserFound(user);
        SetUpRoles(roles);
        SetUpPermissionsFailure();

        var result = await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be(user.Email);
        result.Value.UserName.Should().Be(user.UserName);
        result.Value.Roles.Should().BeEquivalentTo(roles);
        result.Value.Permissions.Should().BeEmpty();
    }

    // ==================== CORRECT USAGE VERIFICATION ====================

    [Fact(DisplayName = "Should call FindByIdAsync with correct user ID")]
    public async Task Handle_ShouldCallFindByIdAsync_WithCorrectUserId()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.Id = userId;

        SetUpAuthenticated(userId);
        SetUpUserFound(user);
        SetUpRoles([]);
        SetUpPermissions([]);

        await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
    }

    [Fact(DisplayName = "Should call GetRolesAsync for user")]
    public async Task Handle_ShouldCallGetRolesAsync_ForUser()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(user.Id);
        SetUpUserFound(user);
        SetUpRoles([]);
        SetUpPermissions([]);

        await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact(DisplayName = "Should call GetEffectiveUserPermissionsAsync with correct user ID")]
    public async Task Handle_ShouldCallGetEffectiveUserPermissionsAsync_WithCorrectUserId()
    {
        var userId = Guid.NewGuid();
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.Id = userId;

        SetUpAuthenticated(userId);
        SetUpUserFound(user);
        SetUpRoles([]);
        SetUpPermissions([]);

        await CreateHandler().Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        _permissionServiceMock.Verify(x => x.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should pass cancellation token to GetEffectiveUserPermissionsAsync")]
    public async Task Handle_ShouldPassCancellationToken_ToGetEffectiveUserPermissionsAsync()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        SetUpAuthenticated(user.Id);
        SetUpUserFound(user);
        SetUpRoles([]);
        SetUpPermissions([]);

        using var cts = new CancellationTokenSource();
        await CreateHandler().Handle(new GetSession.Query(), cts.Token);

        _permissionServiceMock.Verify(
            x => x.GetEffectiveUserPermissionsAsync(It.IsAny<Guid>(), cts.Token),
            Times.Once);
    }
}
