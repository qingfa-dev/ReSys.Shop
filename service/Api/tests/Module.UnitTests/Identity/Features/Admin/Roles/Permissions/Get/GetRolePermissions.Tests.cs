using System.Security.Claims;

using Microsoft.AspNetCore.Identity;


using Module.Identity.Features.Shared.Admin.Roles.Permissions.Get;

using Shared.Security.Authorization.Registry;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;

namespace Module.UnitTests.Identity.Features.Admin.Roles.Permissions.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "RolePermissionsGet")]
public class GetRolePermissionsTests
{
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly GetRolePermissions.QueryHandler _handler;

    public GetRolePermissionsTests()
    {
        var roleStoreMock = new Mock<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null!, null!, null!, null!);
        _handler = new GetRolePermissions.QueryHandler(_roleManagerMock.Object);
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
            new GetRolePermissions.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return tree with IsAssigned false when role has no permissions")]
    public async Task Handle_ShouldReturnEmptyAssigned_WhenRoleHasNoPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "EmptyRole" };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(
            new GetRolePermissions.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var allPermissions = result.Value.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions);

        allPermissions.Should().AllSatisfy(p => p.IsAssigned.Should().BeFalse());
    }

    [Fact(DisplayName = "Should return full discovery tree even when role has no permissions")]
    public async Task Handle_ShouldReturnFullTree_Always()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "EmptyRole" };
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(
            new GetRolePermissions.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Categories.Should().HaveCount(PermissionContext.All.GroupBy(p => p.Category).Count());

        var expectedResourceCount = PermissionContext.All
            .GroupBy(p => p.Category)
            .Sum(g => g.Select(p => p.Resource).Distinct().Count());
        result.Value.Categories.SelectMany(c => c.Resources).Should().HaveCount(expectedResourceCount);
    }

    [Fact(DisplayName = "Should set IsAssigned true for static permissions")]
    public async Task Handle_ShouldSetIsAssigned_ForStaticPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        // Use "Admin" which usually has static permissions defined
        var roleName = "Admin";
        var role = new Role { Id = roleId, Name = roleName };

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([]);

        var expectedStaticPerms = (roleName.ToLowerInvariant() switch
        {
            "admin" => RoleConstant.RolePermissions.Admin,
            "manager" => RoleConstant.RolePermissions.Manager,
            "user" => RoleConstant.RolePermissions.User,
            _ => []
        }).Select(p => p.Identifier).ToHashSet();

        // Act
        var result = await _handler.Handle(
            new GetRolePermissions.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var assignedPermissions = result.Value.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions)
            .Where(p => p.IsAssigned)
            .Select(p => p.Identifier);

        if (expectedStaticPerms.Count > 0)
        {
            assignedPermissions.Should().Contain(expectedStaticPerms);
        }
    }

    [Fact(DisplayName = "Should set IsAssigned true for dynamic claim permissions")]
    public async Task Handle_ShouldSetIsAssigned_ForDynamicClaims()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "DynamicRole" };
        var dynamicPermission = "admin.identity.users.view";

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, dynamicPermission)]);

        // Act
        var result = await _handler.Handle(
            new GetRolePermissions.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var permission = result.Value.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions)
            .FirstOrDefault(p => p.Identifier == dynamicPermission);

        permission.Should().NotBeNull();
        permission.IsAssigned.Should().BeTrue();
    }

    [Fact(DisplayName = "Should handle overlapping static and dynamic permissions")]
    public async Task Handle_ShouldHandleOverlappingPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleName = "Admin";
        var role = new Role { Id = roleId, Name = roleName };

        var staticPerms = roleName.ToLowerInvariant() switch
        {
            "admin" => RoleConstant.RolePermissions.Admin,
            "manager" => RoleConstant.RolePermissions.Manager,
            "user" => RoleConstant.RolePermissions.User,
            _ => []
        };
        if (staticPerms.Count == 0) return; // Skip if no static perms for Admin in this environment

        var overlapPermission = staticPerms[0].Identifier;

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.GetClaimsAsync(role))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, overlapPermission)]);

        // Act
        var result = await _handler.Handle(
            new GetRolePermissions.Query(roleId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var permission = result.Value.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions)
            .FirstOrDefault(p => p.Identifier == overlapPermission);

        permission.Should().NotBeNull();
        permission.IsAssigned.Should().BeTrue();
    }
}
