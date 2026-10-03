using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

using static Module.Identity.Features.Shared.Admin.Roles.Permissions.Assign.AssignRolePermissions;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Permissions.Assign;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RolePermissionsAssign")]
public class AssignRolePermissionsTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CommandHandler>> _loggerMock = new();
    private readonly CommandHandler _handler;

    public AssignRolePermissionsTests()
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
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNull()
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

    [Fact(DisplayName = "Should return NotFound when role doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenRoleNotFound()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(
            new Command(roleId, new Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return SystemRoleProtected when system role updated")]
    public async Task Handle_ShouldReturnSystemRoleProtected_WhenRoleIsSystem()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, IsSystem = true };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(
            new Command(roleId, new Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.SystemRoleProtected.Code);
    }

    [Fact(DisplayName = "Should return Success and do nothing when request permissions are empty")]
    public async Task Handle_ShouldDoNothing_WhenRequestEmpty()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, IsSystem = false };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(
            new Command(roleId, new Request { Permissions = [] }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roleManagerMock.Verify(m => m.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact(DisplayName = "Should return Forbidden when current user lacks the permission to assign")]
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
        result.Errors[0].Code.Should().Be(RoleResult.Failure.AssignDenied(permission).Code);
    }

    [Fact(DisplayName = "Should correctly add new claims and ignore existing ones")]
    public async Task Handle_ShouldOnlyAddMissingClaims()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var role = new Role { Id = roleId, Name = "AdminRole", IsSystem = false };

        var existingClaimValue = "admin.identity.users.list";
        var incomingPermission = "admin.identity.users.view";

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, existingClaimValue)]);

        // Mock permission service: User has permissions
        _permissionServiceMock.Setup(c =>
                c.HasAllPermissionsAsync(userId, It.IsAny<IEnumerable<string>>(),
                    TestContext.Current.CancellationToken))
            .ReturnsAsync(Result<bool>.Ok(true));
        _permissionServiceMock.Setup(c => c.AddRolePermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Ok());

        _roleManagerMock.Setup(m => m.AddClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(m => m.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        var request = new Request { Permissions = [existingClaimValue, incomingPermission] };

        // Act
        var result = await _handler.Handle(
            new Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(
            s => s.AddRolePermissionsAsync(roleId, It.Is<IEnumerable<string>>(p => p.Contains(incomingPermission)),
                TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact(DisplayName = "Should filter out invalid permission identifiers")]
    public async Task Handle_ShouldFilterInvalidIdentifiers()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var role = new Role { Id = roleId, Name = "AdminRole", IsSystem = false };
        var validPermission = "admin.identity.users.view";
        var invalidPermission = "non.existent.permission";

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([]);

        // Mock permission service: User has permissions
        _permissionServiceMock.Setup(c =>
                c.HasAllPermissionsAsync(userId, It.IsAny<IEnumerable<string>>(),
                    TestContext.Current.CancellationToken))
            .ReturnsAsync(true);
        _permissionServiceMock.Setup(c => c.AddRolePermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Ok());

        _roleManagerMock.Setup(m => m.AddClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(m => m.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        var request = new Request { Permissions = [validPermission, invalidPermission] };

        // Act
        var result = await _handler.Handle(
            new Command(roleId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(
            s => s.AddRolePermissionsAsync(roleId, It.Is<IEnumerable<string>>(p => p.Contains(validPermission)),
                TestContext.Current.CancellationToken), Times.Once);
        _roleManagerMock.Verify(m => m.AddClaimAsync(It.IsAny<Role>(), It.IsAny<Claim>()), Times.Never);
        _permissionServiceMock.Verify(x => x.InvalidateRolePermissionsAsync(roleId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}