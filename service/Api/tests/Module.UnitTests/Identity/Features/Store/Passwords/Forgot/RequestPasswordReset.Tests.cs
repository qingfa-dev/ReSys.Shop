using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Module.Identity.Features.Storefront.Passwords.Forgot;
using Module.UnitTests.Identity.Fixtures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Passwords.Forgot;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Accounts")]
public class RequestPasswordResetTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly RequestPasswordReset.CommandHandler _handler;

    public RequestPasswordResetTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _notificationServiceMock = new Mock<INotificationService>();

        var dateTime = new Mock<ISystemDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero));

        _handler = new RequestPasswordReset.CommandHandler(
            _userManagerMock.Object,
            dateTime.Object,
            _notificationServiceMock.Object,
            Options.Create(new NotificationSetting { ApplicationUrl = "https://example.com" }),
            Mock.Of<ILogger<RequestPasswordReset.CommandHandler>>());

        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    [Fact(DisplayName = "Handler: Should return success when user is not found (security)")]
    public async Task Handle_ShouldReturnSuccess_WhenUserNotFound()
    {
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new RequestPasswordReset.Command(new RequestPasswordReset.Request { Email = "notfound@example.com" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return success when user is not active (security)")]
    public async Task Handle_ShouldReturnSuccess_WhenUserNotActive()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.IsActive = false;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        var command = new RequestPasswordReset.Command(new RequestPasswordReset.Request { Email = user.Email! });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: Should generate reset token, return success, and send notification")]
    public async Task Handle_ShouldGenerateResetToken_WhenUserIsActive()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.IsActive = true;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new RequestPasswordReset.Command(new RequestPasswordReset.Request { Email = user.Email! });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _notificationServiceMock.Verify(x => x.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.PasswordResetRequested),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.IsActive = true;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Failed" }));

        var command = new RequestPasswordReset.Command(new RequestPasswordReset.Request { Email = user.Email! });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handler: BuildConfirmPath should create correct URL")]
    public void BuildConfirmPath_ShouldCreateCorrectUrl()
    {
        var userId = Guid.NewGuid();
        var result = RequestPasswordReset.BuildConfirmPath(userId, "tokenABC", "email@test.com");

        result.Should().Be($"reset-password?userId={userId}&token=tokenABC&newEmail=email%40test.com");
    }

    [Fact(DisplayName = "Handler: BuildConfirmPath should URL encode special characters")]
    public void BuildConfirmPath_ShouldUrlEncodeSpecialCharacters()
    {
        var userId = Guid.NewGuid();
        var result = RequestPasswordReset.BuildConfirmPath(userId, "token+with=special", "email+test@test.com");

        result.Should().Contain("token%2Bwith%3Dspecial");
        result.Should().Contain("newEmail=email%2Btest%40test.com");
    }
}
