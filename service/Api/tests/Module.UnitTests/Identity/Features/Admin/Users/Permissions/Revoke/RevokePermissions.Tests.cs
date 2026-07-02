using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Permissions.Revoke;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Permissions.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserPermissionsRevoke")]
public class RevokePermissionsTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<RevokeUserPermissions.CommandHandler>> _loggerMock = new();
    private readonly RevokeUserPermissions.CommandHandler _handler;

    public RevokePermissionsTests()
    {
        Mock<ISystemDateTime> dateTimeMock = new();
        dateTimeMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);

        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _currentUserMock = new Mock<ICurrentUser>();

        _handler = new RevokeUserPermissions.CommandHandler(
            dateTimeMock.Object,
            _userManagerMock.Object,
            _permissionServiceMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "Should return Unauthorized when current user is not authenticated")]
    public async Task Handle_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(Guid.NewGuid(), new RevokeUserPermissions.Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.Unauthorized.Code);
    }

    [Fact(DisplayName = "Should return Unauthorized when current user ID is invalid")]
    public async Task Handle_ShouldReturnUnauthorized_WhenCurrentUserIdIsInvalid()
    {
        // Arrange
        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns("invalid-guid");

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(Guid.NewGuid(), new RevokeUserPermissions.Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.Unauthorized.Code);
    }

    [Fact(DisplayName = "Should return NotFound when target user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(Guid.NewGuid(), new RevokeUserPermissions.Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return Ok when request permissions are empty or invalid")]
    public async Task Handle_ShouldReturnOk_WhenPermissionsAreEmptyOrInvalid()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        var request = new RevokeUserPermissions.Request { Permissions = ["invalid.permission"] };

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(
            s => s.HasAllPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should return RevokeDenied when current user lacks authority")]
    public async Task Handle_ShouldReturnRevokeDenied_WhenCurrentUserLacksAuthority()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var validPermission = "admin.identity.users.view";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s =>
                s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new RevokeUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.RevokeDenied("").Code);
    }

    [Fact(DisplayName = "Should return Ok when user already lacks all requested permissions")]
    public async Task Handle_ShouldReturnOk_WhenUserLacksPermissions()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var validPermission = "admin.identity.users.view";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s =>
                s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([]);

        var request = new RevokeUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(
            s => s.RemoveUserDirectPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should successfully remove permissions and dispatch event")]
    public async Task Handle_ShouldRemovePermissions_WhenValid()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var validPermission = "admin.identity.users.view";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());
        _currentUserMock.Setup(c => c.UserName).Returns("admin");

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s =>
                s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, validPermission)]);

        _permissionServiceMock.Setup(s =>
                s.RemoveUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _userManagerMock.Setup(m => m.UpdateAsync(targetUser))
            .ReturnsAsync(IdentityResult.Success);

        var request = new RevokeUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _permissionServiceMock.Verify(s => s.RemoveUserDirectPermissionsAsync(
                targetUserId,
                It.Is<IEnumerable<string>>(p => p.Contains(validPermission)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userManagerMock.Verify(m => m.UpdateAsync(targetUser), Times.Once);
    }

    [Fact(DisplayName = "Should return failure when RemoveUserDirectPermissionsAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenRemoveDirectPermissionsFails()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var validPermission = "admin.identity.users.view";
        var failureResult = Error.Validation("Remove.Failed", "Failed to remove permission");

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s =>
                s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, validPermission)]);

        _permissionServiceMock.Setup(s =>
                s.RemoveUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var request = new RevokeUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Remove.Failed");
    }

    [Fact(DisplayName = "Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateUserFails()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var validPermission = "admin.identity.users.view";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());
        _currentUserMock.Setup(c => c.UserName).Returns("admin");

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s =>
                s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, validPermission)]);

        _permissionServiceMock.Setup(s =>
                s.RemoveUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _userManagerMock.Setup(m => m.UpdateAsync(targetUser))
            .ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Code = "Update.Failed", Description = "Update failed" }));

        var request = new RevokeUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new RevokeUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Update.Failed");
    }
}