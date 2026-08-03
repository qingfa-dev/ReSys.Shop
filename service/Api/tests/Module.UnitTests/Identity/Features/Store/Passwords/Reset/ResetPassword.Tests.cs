using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Passwords.Reset;
using Module.UnitTests.Identity.Fixtures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Passwords.Reset;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Accounts")]
public class ResetPasswordTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly ResetPassword.CommandHandler _handler;

    public ResetPasswordTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _notificationServiceMock = new Mock<INotificationService>();

        var dateTime = new Mock<ISystemDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero));

        _handler = new ResetPassword.CommandHandler(
            _userManagerMock.Object,
            dateTime.Object,
            _notificationServiceMock.Object,
            Mock.Of<ILogger<ResetPassword.CommandHandler>>());

        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    [Fact(DisplayName = "Handler: Should return InvalidToken when user not found")]
    public async Task Handle_ShouldReturnInvalidToken_WhenUserNotFound()
    {
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new ResetPassword.Command(new ResetPassword.Request { UserId = Guid.NewGuid(), Token = "valid-token", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidToken.Code);
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure when ResetPasswordAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenResetPasswordAsyncFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, "valid-token", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

        var command = new ResetPassword.Command(new ResetPassword.Request { UserId = user.Id, Token = "valid-token", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return success and send notification when password reset is successful")]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordResetIsSuccessful()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, "valid-token", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ResetPassword.Command(new ResetPassword.Request { UserId = user.Id, Token = "valid-token", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be(UserResult.Success.PasswordReset);
        _notificationServiceMock.Verify(x => x.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.PasswordChanged),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, "valid-token", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Failed to update." }));

        var command = new ResetPassword.Command(new ResetPassword.Request { UserId = user.Id, Token = "valid-token", NewPassword = "NewPass1!" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
