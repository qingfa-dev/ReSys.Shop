using Shared.Security.Identity.Domain.Roles;

namespace Shared.UnitTests.Security.Identity.Domain.Roles;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Roles")]
public sealed class RoleConstantTests
{
    [Fact(DisplayName = "RoleConstant: Admin should have all permissions")]
    public void Admin_ShouldHaveAllPermissions()
    {
        RoleConstant.RolePermissions.Admin.Should().NotBeNullOrEmpty();
        RoleConstant.RolePermissions.Admin.Should().HaveCountGreaterThan(10);
    }

    [Fact(DisplayName = "RoleConstant: Manager and User should have permission subsets")]
    public void ManagerAndUser_ShouldBeSubsetsOfAdmin()
    {
        RoleConstant.RolePermissions.Manager.Should().NotBeNull();
        RoleConstant.RolePermissions.User.Should().NotBeNull();
    }
}
