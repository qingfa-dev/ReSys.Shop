using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Users.Roles.Sync;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Roles.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserRoles")]
public class SyncUserRolesTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ISystemDateTime> _dateTimeMock;
    private readonly Mock<ILogger<SyncUserRoles.CommandHandler>> _loggerMock = new();
    private readonly SyncUserRoles.CommandHandler _handler;

    public SyncUserRolesTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _roleManagerMock = IdentityMocks.CreateRoleManagerMock<Role>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<ISystemDateTime>();

        _handler = new SyncUserRoles.CommandHandler(
            _dateTimeMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _permissionServiceMock.Object,
            _loggerMock.Object);

        _dateTimeMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);
        _currentUserMock.Setup(x => x.UserName).Returns("admin");
    }

    [Fact(DisplayName = "Handler: Should return NotFound when user is not found")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new SyncUserRoles.Command(Guid.NewGuid(), new SyncUserRoles.Request { Roles = ["Admin"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should sync roles (add and remove) and raise event")]
    public async Task Handle_ShouldSyncRoles_WhenValid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["OldRole"]);
        _roleManagerMock.Setup(x => x.RoleExistsAsync("NewRole")).ReturnsAsync(true);

        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("OldRole"))))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("NewRole"))))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new SyncUserRoles.Command(user.Id, new SyncUserRoles.Request { Roles = ["NewRole"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _userManagerMock.Verify(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return RoleNotFound when adding a non-existent role")]
    public async Task Handle_ShouldReturnError_WhenRoleDoesNotExist()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync([]);
        _roleManagerMock.Setup(x => x.RoleExistsAsync("InvalidRole")).ReturnsAsync(false);

        var command = new SyncUserRoles.Command(user.Id, new SyncUserRoles.Request { Roles = ["InvalidRole"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(RoleResult.Failure.NotFound.Code);
    }
}
