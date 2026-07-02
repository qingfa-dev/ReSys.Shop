using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using Shared.Security.Authorization.Permissions.Services;

namespace Module.UnitTests.Identity.Fixtures;

public static class IdentityMocks
{
    public static Mock<UserManager<TUser>> CreateUserManagerMock<TUser>() where TUser : class
        => new Mock<UserManager<TUser>>(
            Mock.Of<IUserStore<TUser>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

    public static Mock<RoleManager<TRole>> CreateRoleManagerMock<TRole>() where TRole : class
        => new Mock<RoleManager<TRole>>(
            Mock.Of<IRoleStore<TRole>>(),
            null!,
            null!,
            null!,
            null!);

    public static Mock<SignInManager<TUser>> CreateSignInManagerMock<TUser>(
        Mock<UserManager<TUser>> userManagerMock) where TUser : class
        => new Mock<SignInManager<TUser>>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<TUser>>(),
            null!,
            null!,
            null!,
            null!);

    public static Mock<ICurrentUser> CreateCurrentUserMock(Guid userId)
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(x => x.UserId).Returns(userId.ToString());
        return mock;
    }

    public static Mock<ICurrentUser> CreateAuthenticatedCurrentUserMock(
        Guid userId,
        string? email = null,
        string? userName = null)
    {
        var mock = CreateCurrentUserMock(userId);
        mock.Setup(x => x.IsAuthenticated).Returns(true);
        mock.Setup(x => x.Email).Returns(email ?? "test@example.com");
        mock.Setup(x => x.UserName).Returns(userName ?? email ?? "test@example.com");
        return mock;
    }

    public static Mock<IPermissionService> CreatePermissionServiceMock(
        HashSet<string>? permissions = null)
    {
        var mock = new Mock<IPermissionService>();
        mock.Setup(x => x.GetEffectiveUserPermissionsAsync(It.IsAny<Guid>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(permissions ?? []);
        return mock;
    }

    public static Mock<IPermissionService> CreatePermissionCacheMockWithPermissions(
        params string[] permissions)
    {
        return CreatePermissionServiceMock(permissions.ToHashSet());
    }
}
