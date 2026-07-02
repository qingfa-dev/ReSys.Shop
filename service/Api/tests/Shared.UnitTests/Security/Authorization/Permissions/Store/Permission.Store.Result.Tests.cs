using Shared.Security.Authorization.Permissions.Store;

namespace Shared.UnitTests.Security.Authorization.Permissions.Store;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionStore")]
public sealed class PermissionStoreResultTests
{
    [Fact(DisplayName = "Success.Retrieved should return expected message")]
    public void Success_Retrieved_ShouldReturnExpectedMessage()
    {
        PermissionStoreResult.Success.Retrieved.Should().Be("Permissions retrieved successfully.");
    }

    [Fact(DisplayName = "Failure.Unexpected should return error with given code and message")]
    public void Failure_Unexpected_ShouldReturnError()
    {
        Error error = PermissionStoreResult.Failure.Unexpected("Store.Fail", "Database error.");

        error.Code.Should().Be("Store.Fail");
        error.Message.Should().Be("Database error.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }
}
