using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Authorization.Requirements;

namespace Shared.UnitTests.Security.Authorization.Requirements;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "PermissionRequirementAuthorizationHandler")]
public sealed class PermissionRequirementAuthorizationHandlerTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ILogger<PermissionRequirementAuthorizationHandler>> _loggerMock;
    private readonly PermissionRequirementAuthorizationHandler _sut;

    public PermissionRequirementAuthorizationHandlerTests()
    {
        _permissionServiceMock = new Mock<IPermissionService>();
        _loggerMock = new Mock<ILogger<PermissionRequirementAuthorizationHandler>>();

        _sut = new PermissionRequirementAuthorizationHandler(
            _permissionServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "PermissionRequirementAuthorizationHandler: Should succeed immediately for admin user, bypassing permission service")]
    public async Task HandleRequirementAsync_ShouldSucceedImmediately_WhenUserIsAdmin()
    {
        PermissionRequirement requirement = new("any.permission");
        ClaimsPrincipal principal = CreatePrincipal(nameId: Guid.NewGuid().ToString(), isAdmin: true);
        AuthorizationHandlerContext context = new([requirement], principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        _permissionServiceMock.Verify(
            x => x.GetEffectiveUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact(DisplayName = "PermissionRequirementAuthorizationHandler: Should succeed when non-admin user has the required permission")]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenNonAdminUserHasPermission()
    {
        Guid userId = Guid.NewGuid();
        PermissionRequirement requirement = new("any.permission");
        ClaimsPrincipal principal = CreatePrincipal(nameId: userId.ToString(), isAdmin: false);
        AuthorizationHandlerContext context = new([requirement], principal, null);

        _permissionServiceMock
            .Setup(x => x.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(new HashSet<string> { "any.permission" }));

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        _permissionServiceMock.Verify(
            x => x.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact(DisplayName = "PermissionRequirementAuthorizationHandler: Should fail when non-admin user lacks the required permission")]
    public async Task HandleRequirementAsync_ShouldFail_WhenNonAdminUserLacksPermission()
    {
        Guid userId = Guid.NewGuid();
        PermissionRequirement requirement = new("any.permission");
        ClaimsPrincipal principal = CreatePrincipal(nameId: userId.ToString(), isAdmin: false);
        AuthorizationHandlerContext context = new([requirement], principal, null);

        _permissionServiceMock
            .Setup(x => x.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok(new HashSet<string>()));

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact(DisplayName = "PermissionRequirementAuthorizationHandler: Should not succeed for unauthenticated user")]
    public async Task HandleRequirementAsync_ShouldNotSucceed_WhenUserIsUnauthenticated()
    {
        PermissionRequirement requirement = new("any.permission");
        ClaimsPrincipal principal = new(new ClaimsIdentity());
        AuthorizationHandlerContext context = new([requirement], principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _permissionServiceMock.Verify(
            x => x.GetEffectiveUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact(DisplayName = "PermissionRequirementAuthorizationHandler: Should not succeed when authenticated user has no NameIdentifier claim")]
    public async Task HandleRequirementAsync_ShouldNotSucceed_WhenUserHasNoNameIdentifier()
    {
        PermissionRequirement requirement = new("any.permission");
        List<Claim> claims = [new Claim(ClaimTypes.Name, "testuser")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        AuthorizationHandlerContext context = new([requirement], principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact(DisplayName = "PermissionRequirementAuthorizationHandler: Should succeed for admin user even when permission service would fail")]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenAdminUserRegardlessOfServiceState()
    {
        PermissionRequirement requirement = new("any.permission");
        ClaimsPrincipal principal = CreatePrincipal(nameId: Guid.NewGuid().ToString(), isAdmin: true);
        AuthorizationHandlerContext context = new([requirement], principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    private static ClaimsPrincipal CreatePrincipal(string? nameId = null, bool isAdmin = false)
    {
        List<Claim> claims = [];
        if (nameId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId));
        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        ClaimsIdentity identity = new(claims, "test");
        return new ClaimsPrincipal(identity);
    }
}
