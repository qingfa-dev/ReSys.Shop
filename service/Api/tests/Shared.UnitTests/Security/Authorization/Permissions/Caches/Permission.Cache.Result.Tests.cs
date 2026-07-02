using Shared.Security.Authorization.Permissions.Caches;

namespace Shared.UnitTests.Security.Authorization.Permissions.Caches;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionCache")]
public sealed class PermissionCacheResultTests
{
    [Fact(DisplayName = "Success.Retrieved should return expected message")]
    public void Success_Retrieved_ShouldReturnExpectedMessage()
    {
        PermissionCacheResult.Success.Retrieved.Should().Be("User permissions retrieved from cache.");
    }

    [Fact(DisplayName = "Success.RoleRetrieved should return expected message")]
    public void Success_RoleRetrieved_ShouldReturnExpectedMessage()
    {
        PermissionCacheResult.Success.RoleRetrieved.Should().Be("Role permissions retrieved from cache.");
    }

    [Fact(DisplayName = "Success.Cached should return expected message")]
    public void Success_Cached_ShouldReturnExpectedMessage()
    {
        PermissionCacheResult.Success.Cached.Should().Be("Permissions cached successfully.");
    }

    [Fact(DisplayName = "Success.Invalidated should return expected message")]
    public void Success_Invalidated_ShouldReturnExpectedMessage()
    {
        PermissionCacheResult.Success.Invalidated.Should().Be("User cache invalidated successfully.");
    }

    [Fact(DisplayName = "Success.RoleInvalidated should return expected message")]
    public void Success_RoleInvalidated_ShouldReturnExpectedMessage()
    {
        PermissionCacheResult.Success.RoleInvalidated.Should().Be("Role cache invalidated successfully.");
    }

    [Fact(DisplayName = "Success.AllInvalidated should return expected message")]
    public void Success_AllInvalidated_ShouldReturnExpectedMessage()
    {
        PermissionCacheResult.Success.AllInvalidated.Should().Be("All permission caches invalidated.");
    }

    [Fact(DisplayName = "Failure.Unexpected should return error with given code and message")]
    public void Failure_Unexpected_ShouldReturnError()
    {
        Error error = PermissionCacheResult.Failure.Unexpected("Cache.Fail", "Something went wrong.");

        error.Code.Should().Be("Cache.Fail");
        error.Message.Should().Be("Something went wrong.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }
}
