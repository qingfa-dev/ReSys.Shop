using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Roles.Permissions.Revoke;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

using static Module.Identity.Features.Shared.Admin.Roles.Permissions.Revoke.RevokeRolePermissions;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Permissions.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RolePermissionsRevoke")]
public class RevokePermissionsTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CommandHandler>> _loggerMock = new();
    private readonly CommandHandler _handler;

    public RevokePermissionsTests()
    {
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);
        _permissionServiceMock = new Mock<IPermissionService>();
        _currentUserMock = new Mock<ICurrentUser>();
        Mock<ISystemDateTime> dateTimeMock = new();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _handler = new CommandHandler(
            dateTimeMock.Object,
            _roleManagerMock.Object,
            _permissionServiceMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "Should return Unauthorized when current user is not authenticated")]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(
            new Command(Guid.NewGuid(), new Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.Unauthorized.Code);
    }

    [Fact(DisplayName = "Should return Forbidden when current user lacks the permission to revoke")]
    public async Task Handle_ShouldReturnForbidden_WhenUserLacksPermission()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var role = new Role { Id = roleId, Name = "AdminRole", IsSystem = false };
        var permission = "admin.identity.users.view";

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        // Mock permission service: User lacks permissions
        _permissionServiceMock.Setup(c =>
                c.HasAllPermissionsAsync(userId, It.IsAny<IEnumerable<string>>(),
                    TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<bool>.Ok(false));

        var request = new Request { Permissions = [permission] };

        // Act
        var result = await _handler.Handle(
            new Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.RevokeDenied(permission).Code);
    }

    [Fact(DisplayName = "Should correctly revoke permissions")]
    public async Task Handle_ShouldRevokeExistingPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var role = new Role { Id = roleId, Name = "AdminRole", IsSystem = false };

        var permissionToRevoke = "admin.identity.users.view";
        var otherPermission = "admin.identity.users.list";
        var claimToRemove = new Claim(PermissionMetadataConstant.ClaimType, permissionToRevoke);

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([claimToRemove, new Claim(PermissionMetadataConstant.ClaimType, otherPermission)]);

        // Mock permission service: User has permissions to revoke
        _permissionServiceMock.Setup(c =>
                c.HasAllPermissionsAsync(userId, It.IsAny<IEnumerable<string>>(),
                    TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<bool>.Ok(true));
        _permissionServiceMock.Setup(c => c.RemoveRolePermissionsAsync(It.IsAny<Guid>(),
                It.IsAny<IEnumerable<string>>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Ok());

        _roleManagerMock.Setup(m => m.RemoveClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(m => m.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        var request = new Request { Permissions = [permissionToRevoke] };

        // Act
        var result = await _handler.Handle(
            new Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(
            s => s.RemoveRolePermissionsAsync(roleId, It.Is<IEnumerable<string>>(p => p.Contains(permissionToRevoke)),
                TestContext.Current.CancellationToken), Times.Once);
        _permissionServiceMock.Verify(x => x.InvalidateRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Should ignore permissions that the role doesn't have")]
    public async Task Handle_ShouldIgnoreNonExistentPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var role = new Role { Id = roleId, Name = "AdminRole", IsSystem = false };
        var nonExistentPermission = "non.existent";

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([]);

        // Mock permission service: User has permissions
        _permissionServiceMock.Setup(c =>
                c.HasAllPermissionsAsync(userId, It.IsAny<IEnumerable<string>>(),
                    TestContext.Current.CancellationToken))
            .ReturnsAsync(true);

        var request = new Request { Permissions = [nonExistentPermission] };

        // Act
        var result = await _handler.Handle(
            new Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roleManagerMock.Verify(m => m.RemoveClaimAsync(role, It.IsAny<Claim>()), Times.Never);
    }
}