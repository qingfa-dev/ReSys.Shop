using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Admin.Users.Status;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Status;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserToggleStatus")]
public class ToggleUserStatusTests
{
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ICurrentUser> _currentUserMock = null!;
    private readonly Mock<ILogger<ToggleUserStatus.CommandHandler>> _loggerMock = new();

    private ToggleUserStatus.CommandHandler CreateCommandHandler()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");
        _currentUserMock.Setup(x => x.UserId).Returns("A0000000-0000-0000-0000-000000000000");

        return new ToggleUserStatus.CommandHandler(_userManagerMock.Object, _currentUserMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "UseCase: Should toggle status when user exists")]
    public async Task Handle_ShouldToggleStatus_WhenUserExists()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(new ToggleUserStatus.Command(userId), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();

        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact(DisplayName = "UseCase: Should return NotFound when user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

        // Act
        var result = await handler.Handle(new ToggleUserStatus.Command(userId), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "UseCase: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Fail" }));

        // Act
        var result = await handler.Handle(new ToggleUserStatus.Command(userId), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should succeed when user is inactive")]
    public async Task Handle_ShouldSucceed_WhenUserIsInactive()
    {
        // Arrange
        var handler = CreateCommandHandler();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await handler.Handle(new ToggleUserStatus.Command(userId), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
}
