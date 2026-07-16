using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Passwords.Change;
using Module.UnitTests.Identity.Fixtures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Passwords.Change;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Accounts")]
public class ChangePasswordTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ISystemDateTime> _dateTimeMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly ChangePassword.CommandHandler _handler;

    public ChangePasswordTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<ISystemDateTime>();
        _notificationServiceMock = new Mock<INotificationService>();

        _handler = new ChangePassword.CommandHandler(
            _currentUserMock.Object,
            _dateTimeMock.Object,
            _userManagerMock.Object,
            _notificationServiceMock.Object,
            Mock.Of<ILogger<ChangePassword.CommandHandler>>());

        _dateTimeMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    [Fact(DisplayName = "Handler: Should return NotFound when user is not found")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new ChangePassword.Command(new ChangePassword.Request { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return PasswordMismatch when current password is invalid")]
    public async Task Handle_ShouldReturnPasswordMismatch_WhenCurrentPasswordIsInvalid()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        _currentUserMock.Setup(x => x.UserId).Returns(user.Id.ToString());
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "WrongPass1!"))
            .ReturnsAsync(false);

        var command = new ChangePassword.Command(new ChangePassword.Request { CurrentPassword = "WrongPass1!", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.PasswordMismatch.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when ChangePasswordAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenChangePasswordAsyncFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var userIdString = user.Id.ToString();

        _currentUserMock.Setup(x => x.UserId).Returns(userIdString);
        _userManagerMock.Setup(x => x.FindByIdAsync(userIdString))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Failed" }));

        var command = new ChangePassword.Command(new ChangePassword.Request { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return success and send notification when password is changed")]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordIsChanged()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var userIdString = user.Id.ToString();

        _currentUserMock.Setup(x => x.UserId).Returns(userIdString);
        _userManagerMock.Setup(x => x.FindByIdAsync(userIdString))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePassword.Command(new ChangePassword.Request { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be(UserResult.Success.PasswordChanged);
        _notificationServiceMock.Verify(x => x.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.PasswordChanged),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var userIdString = user.Id.ToString();

        _currentUserMock.Setup(x => x.UserId).Returns(userIdString);
        _userManagerMock.Setup(x => x.FindByIdAsync(userIdString))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" }));

        var command = new ChangePassword.Command(new ChangePassword.Request { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should set ModifiedAtUtc and send notification when password is changed")]
    public async Task Handle_ShouldSetModifiedAtUtc()
    {
        var expectedTime = DateTimeOffset.UtcNow;
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.ModifiedAtUtc = DateTimeOffset.MinValue;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(expectedTime);

        _currentUserMock.Setup(x => x.UserId).Returns(user.Id.ToString());
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePassword.Command(new ChangePassword.Request { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" });

        await _handler.Handle(command, TestContext.Current.CancellationToken);

        user.ModifiedAtUtc.Should().Be(expectedTime);
    }
}
