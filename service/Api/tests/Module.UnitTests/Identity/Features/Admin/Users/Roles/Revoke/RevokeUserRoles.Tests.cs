using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Roles.Revoke;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Roles.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserRoles")]
public class RevokeUserRolesTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ISystemDateTime> _dateTimeMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ILogger<RevokeUserRoles.CommandHandler>> _loggerMock = new();
    private readonly RevokeUserRoles.CommandHandler _handler;

    public RevokeUserRolesTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<ISystemDateTime>();
        _permissionServiceMock = new Mock<IPermissionService>();

        _handler = new RevokeUserRoles.CommandHandler(
            _dateTimeMock.Object,
            _userManagerMock.Object,
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

        var command = new RevokeUserRoles.Command(Guid.NewGuid(), new RevokeUserRoles.Request { Roles = ["Admin"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return success when no roles to remove")]
    public async Task Handle_ShouldReturnOk_WhenNoRolesToRemove()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var command = new RevokeUserRoles.Command(user.Id, new RevokeUserRoles.Request { Roles = ["NonExistent"] });
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "NonExistent")).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should revoke roles and raise event")]
    public async Task Handle_ShouldRevokeRoles_WhenValid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new RevokeUserRoles.Command(user.Id, new RevokeUserRoles.Request { Roles = ["Admin"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin"))), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when RemoveFromRolesAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenIdentityFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Identity error" }));

        var command = new RevokeUserRoles.Command(user.Id, new RevokeUserRoles.Request { Roles = ["Admin"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
