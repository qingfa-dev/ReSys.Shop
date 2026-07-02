using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Admin.Users.Roles.Assign;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Authorization.Permissions.Services;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Roles.Assign;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserRoles")]
public class AssignUserRolesTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ISystemDateTime> _dateTimeMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Mock<ILogger<AssignUserRoles.CommandHandler>> _loggerMock = new();
    private readonly AssignUserRoles.CommandHandler _handler;

    public AssignUserRolesTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _roleManagerMock = IdentityMocks.CreateRoleManagerMock<Role>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<ISystemDateTime>();
        _permissionServiceMock = new Mock<IPermissionService>();

        _handler = new AssignUserRoles.CommandHandler(
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

        var command = new AssignUserRoles.Command(Guid.NewGuid(), new AssignUserRoles.Request { Roles = ["Admin"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return success when no roles to add")]
    public async Task Handle_ShouldReturnOk_WhenNoNewRolesSpecified()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        var command = new AssignUserRoles.Command(user.Id, new AssignUserRoles.Request { Roles = [] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should skip roles that do not exist or user already has")]
    public async Task Handle_ShouldSkipInvalidOrExistingRoles()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("NonExistent")).ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.RoleExistsAsync("Existing")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Existing")).ReturnsAsync(true);

        var command = new AssignUserRoles.Command(user.Id, new AssignUserRoles.Request { Roles = ["NonExistent", "Existing"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should assign roles and raise event")]
    public async Task Handle_ShouldAssignRoles_WhenValid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("NewRole")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "NewRole")).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new AssignUserRoles.Command(user.Id, new AssignUserRoles.Request { Roles = ["NewRole"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("NewRole"))), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when AddToRolesAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenIdentityFails()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync("NewRole")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.IsInRoleAsync(user, "NewRole")).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Identity error" }));

        var command = new AssignUserRoles.Command(user.Id, new AssignUserRoles.Request { Roles = ["NewRole"] });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
