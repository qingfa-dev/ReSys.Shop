using Shared.Security.Authorization.Permissions.Services;

namespace Shared.UnitTests.Security.Authorization.Permissions.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionService")]
public sealed class PermissionServiceResultTests
{
    [Fact(DisplayName = "Success.Resolved should return expected message")]
    public void Success_Resolved_ShouldReturnExpectedMessage()
    {
        PermissionServiceResult.Success.Resolved.Should().Be("User effective permissions resolved.");
    }

    [Fact(DisplayName = "Success.RoleResolved should return expected message")]
    public void Success_RoleResolved_ShouldReturnExpectedMessage()
    {
        PermissionServiceResult.Success.RoleResolved.Should().Be("Role permissions resolved.");
    }

    [Fact(DisplayName = "Success.Invalidated should return expected message")]
    public void Success_Invalidated_ShouldReturnExpectedMessage()
    {
        PermissionServiceResult.Success.Invalidated.Should().Be("Permissions invalidated successfully.");
    }

    [Fact(DisplayName = "Success.Added should return expected message")]
    public void Success_Added_ShouldReturnExpectedMessage()
    {
        PermissionServiceResult.Success.Added.Should().Be("Permissions added successfully.");
    }

    [Fact(DisplayName = "Success.Removed should return expected message")]
    public void Success_Removed_ShouldReturnExpectedMessage()
    {
        PermissionServiceResult.Success.Removed.Should().Be("Permissions removed successfully.");
    }

    [Fact(DisplayName = "Failure.Unexpected should return error with given code and message")]
    public void Failure_Unexpected_ShouldReturnError()
    {
        Error error = PermissionServiceResult.Failure.Unexpected("Service.Fail", "Operation failed.");

        error.Code.Should().Be("Service.Fail");
        error.Message.Should().Be("Operation failed.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }
}
