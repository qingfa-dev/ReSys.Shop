using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;

using static Module.Identity.Features.Admin.Roles.Permissions.Sync.SyncRolePermissions;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Permissions.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RolePermissionsSync")]
public class SyncPermissionsTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CommandHandler>> _loggerMock = new();
    private readonly CommandHandler _handler;

    public SyncPermissionsTests()
    {
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);
        _permissionServiceMock = new Mock<IPermissionService>();
        _currentUserMock = new Mock<ICurrentUser>();
        Mock<ISystemDateTime> dateTimeMock = new();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new CommandHandler(
            dateTimeMock.Object,
            _roleManagerMock.Object,
            _permissionServiceMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "Should correctly sync permissions (add and remove)")]
    public async Task Handle_ShouldSyncPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var role = new Role { Id = roleId, Name = "AdminRole", IsSystem = false };

        var keptPermission = "admin.identity.users.view";
        var removedPermission = "admin.identity.users.create";
        var addedPermission = "admin.identity.users.update";

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([
                new Claim(PermissionMetadataConstant.ClaimType, keptPermission),
                new Claim(PermissionMetadataConstant.ClaimType, removedPermission)
            ]);

        // Mock permission service: User has all affected permissions
        _permissionServiceMock.Setup(c =>
                c.HasAllPermissionsAsync(userId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        _permissionServiceMock.Setup(s => s.AddRolePermissionsAsync(roleId,
                It.Is<IEnumerable<string>>(p => p.Contains(addedPermission)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _permissionServiceMock.Setup(s => s.RemoveRolePermissionsAsync(roleId,
                It.Is<IEnumerable<string>>(p => p.Contains(removedPermission)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _roleManagerMock.Setup(m => m.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        var request = new Request { Permissions = [keptPermission, addedPermission] };

        // Act
        var result = await _handler.Handle(
            new Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(
            s => s.AddRolePermissionsAsync(roleId,
                It.Is<IEnumerable<string>>(p => p.Count() == 1 && p.Contains(addedPermission)),
                It.IsAny<CancellationToken>()), Times.Once);
        _permissionServiceMock.Verify(
            s => s.RemoveRolePermissionsAsync(roleId,
                It.Is<IEnumerable<string>>(p => p.Count() == 1 && p.Contains(removedPermission)),
                It.IsAny<CancellationToken>()), Times.Once);
        _permissionServiceMock.Verify(x => x.InvalidateRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}