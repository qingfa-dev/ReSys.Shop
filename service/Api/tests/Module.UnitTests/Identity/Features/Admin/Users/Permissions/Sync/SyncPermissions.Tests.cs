using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Users.Permissions.Sync;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Permissions.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserPermissionsSync")]
public class SyncPermissionsTests
{
    private readonly Mock<ISystemDateTime> _dateTimeMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<SyncUserPermissions.CommandHandler>> _loggerMock = new();
    private readonly SyncUserPermissions.CommandHandler _handler;

    public SyncPermissionsTests()
    {
        _dateTimeMock = new Mock<ISystemDateTime>();
        _dateTimeMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);

        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _currentUserMock = new Mock<ICurrentUser>();

        _handler = new SyncUserPermissions.CommandHandler(
            _dateTimeMock.Object,
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
            new SyncUserPermissions.Command(Guid.NewGuid(), new SyncUserPermissions.Request()),
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
            new SyncUserPermissions.Command(Guid.NewGuid(), new SyncUserPermissions.Request()),
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
            new SyncUserPermissions.Command(Guid.NewGuid(), new SyncUserPermissions.Request()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Should return Ok when request permissions equal existing permissions")]
    public async Task Handle_ShouldReturnOk_WhenPermissionsEqualExisting()
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

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, validPermission)]);

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new SyncUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _permissionServiceMock.Verify(s => s.AddUserDirectPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        _permissionServiceMock.Verify(s => s.RemoveUserDirectPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should return AssignDenied when current user lacks authority for added permissions")]
    public async Task Handle_ShouldReturnAssignDenied_WhenCurrentUserLacksAuthorityForAdds()
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

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new SyncUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.AssignDenied("").Code);
    }

    [Fact(DisplayName = "Should return RevokeDenied when current user lacks authority for removed permissions")]
    public async Task Handle_ShouldReturnRevokeDenied_WhenCurrentUserLacksAuthorityForRemoves()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var existingPermission = "admin.identity.users.view";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, existingPermission)]);

        // They lack authority for the removal
        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new SyncUserPermissions.Request { Permissions = [] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.RevokeDenied("").Code);
    }

    [Fact(DisplayName = "Should successfully sync permissions (add and remove) and dispatch event")]
    public async Task Handle_ShouldSyncPermissions_WhenValid()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };

        var toAdd = "admin.identity.users.create";
        var toRemove = "admin.identity.users.view";
        var toKeep = "admin.identity.users.update";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());
        _currentUserMock.Setup(c => c.UserName).Returns("admin");

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([
                new Claim(PermissionMetadataConstant.ClaimType, toRemove),
                new Claim(PermissionMetadataConstant.ClaimType, toKeep)
            ]);

        _permissionServiceMock.Setup(s => s.AddUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _permissionServiceMock.Setup(s => s.RemoveUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _userManagerMock.Setup(m => m.UpdateAsync(targetUser))
            .ReturnsAsync(IdentityResult.Success);

        var request = new SyncUserPermissions.Request { Permissions = [toAdd, toKeep] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _permissionServiceMock.Verify(s => s.AddUserDirectPermissionsAsync(
            targetUserId,
            It.Is<IEnumerable<string>>(p => p.Contains(toAdd) && !p.Contains(toKeep)),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _permissionServiceMock.Verify(s => s.RemoveUserDirectPermissionsAsync(
            targetUserId,
            It.Is<IEnumerable<string>>(p => p.Contains(toRemove) && !p.Contains(toKeep)),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _userManagerMock.Verify(m => m.UpdateAsync(targetUser), Times.Once);
    }

    [Fact(DisplayName = "Should successfully sync permissions when only adding")]
    public async Task Handle_ShouldSyncPermissions_WhenOnlyAdding()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var toAdd = "admin.identity.users.create";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([]);

        _permissionServiceMock.Setup(s => s.AddUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _userManagerMock.Setup(m => m.UpdateAsync(targetUser))
            .ReturnsAsync(IdentityResult.Success);

        var request = new SyncUserPermissions.Request { Permissions = [toAdd] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _permissionServiceMock.Verify(s => s.AddUserDirectPermissionsAsync(
            targetUserId,
            It.Is<IEnumerable<string>>(p => p.Contains(toAdd)),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _permissionServiceMock.Verify(s => s.RemoveUserDirectPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should successfully sync permissions when only removing")]
    public async Task Handle_ShouldSyncPermissions_WhenOnlyRemoving()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var toRemove = "admin.identity.users.view";

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, toRemove)]);

        _permissionServiceMock.Setup(s => s.RemoveUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _userManagerMock.Setup(m => m.UpdateAsync(targetUser))
            .ReturnsAsync(IdentityResult.Success);

        var request = new SyncUserPermissions.Request { Permissions = [] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _permissionServiceMock.Verify(s => s.AddUserDirectPermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);

        _permissionServiceMock.Verify(s => s.RemoveUserDirectPermissionsAsync(
            targetUserId,
            It.Is<IEnumerable<string>>(p => p.Contains(toRemove)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Should return failure when AddUserDirectPermissionsAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenAddDirectPermissionsFails()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetUser = new User { Id = targetUserId, UserName = "testuser" };
        var validPermission = "admin.identity.users.view";
        var failureResult = Error.Validation("Add.Failed", "Failed to add permission");

        _currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(c => c.UserId).Returns(currentUserId.ToString());

        _userManagerMock.Setup(m => m.FindByIdAsync(targetUserId.ToString()))
            .ReturnsAsync(targetUser);

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Ok(true));

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([]);

        _permissionServiceMock.Setup(s => s.AddUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var request = new SyncUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Add.Failed");
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

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([new Claim(PermissionMetadataConstant.ClaimType, validPermission)]);

        _permissionServiceMock.Setup(s => s.RemoveUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var request = new SyncUserPermissions.Request { Permissions = [] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
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

        _permissionServiceMock.Setup(s => s.HasAllPermissionsAsync(currentUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetClaimsAsync(targetUser))
            .ReturnsAsync([]);

        _permissionServiceMock.Setup(s => s.AddUserDirectPermissionsAsync(targetUserId, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _userManagerMock.Setup(m => m.UpdateAsync(targetUser))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Update.Failed", Description = "Update failed" }));

        var request = new SyncUserPermissions.Request { Permissions = [validPermission] };

        // Act
        var result = await _handler.Handle(
            new SyncUserPermissions.Command(targetUserId, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Update.Failed");
    }
}
