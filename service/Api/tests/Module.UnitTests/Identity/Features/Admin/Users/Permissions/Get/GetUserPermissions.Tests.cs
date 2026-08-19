using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Users.Permissions.Get;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Permissions.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserPermissionsGet")]
public class GetUserPermissionsTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly GetUserPermissions.QueryHandler _handler;

    public GetUserPermissionsTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _permissionServiceMock = new Mock<IPermissionService>();

        _handler = new GetUserPermissions.QueryHandler(_userManagerMock.Object, _permissionServiceMock.Object);
    }

    [Fact(DisplayName = "Should return NotFound when user doesn't exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(
            new GetUserPermissions.Query(userId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return failure when permission service fails")]
    public async Task Handle_ShouldReturnFailure_WhenPermissionServiceFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "testuser" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _permissionServiceMock.Setup(m => m.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation("Service.Error", "Failed to get permissions"));

        // Act
        var result = await _handler.Handle(
            new GetUserPermissions.Query(userId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Service.Error");
    }

    [Fact(DisplayName = "Should return tree with IsAssigned false when user has no permissions")]
    public async Task Handle_ShouldReturnEmptyAssigned_WhenUserHasNoPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "testuser" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _permissionServiceMock.Setup(m => m.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        // Act
        var result = await _handler.Handle(
            new GetUserPermissions.Query(userId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var allPermissions = result.Value.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions);

        allPermissions.Should().AllSatisfy(p => p.IsAssigned.Should().BeFalse());
    }

    [Fact(DisplayName = "Should set IsAssigned true for returned permissions")]
    public async Task Handle_ShouldSetIsAssigned_ForPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "testuser" };
        var permissionIdentifier = "admin.identity.users.view";

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _permissionServiceMock.Setup(m => m.GetEffectiveUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<HashSet<string>>.Ok([permissionIdentifier]));

        // Act
        var result = await _handler.Handle(
            new GetUserPermissions.Query(userId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var permission = result.Value.Categories
            .SelectMany(c => c.Resources)
            .SelectMany(r => r.Permissions)
            .FirstOrDefault(p => p.Identifier == permissionIdentifier);

        permission.Should().NotBeNull();
        permission.IsAssigned.Should().BeTrue();
    }
}