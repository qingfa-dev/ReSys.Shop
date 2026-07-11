using System.Text;

using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Store.Emails.Confirm;
using Module.Profile.Features.Store.Profiles.Create;
using Module.UnitTests.Identity.Fixtures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Emails.Confirm;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "EmailConfirmation")]
public class ConfirmEmailTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ILogger<ConfirmEmail.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IMediator> _mediatorMock;

    public ConfirmEmailTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _loggerMock = new Mock<ILogger<ConfirmEmail.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _notificationServiceMock = new Mock<INotificationService>();
        _mediatorMock = new Mock<IMediator>();

        _currentUserMock.Setup(x => x.UserName).Returns("admin");
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateProfile.Response());
    }

    private ConfirmEmail.CommandHandler CreateHandler()
        => new(
            _userManagerMock.Object,
            _currentUserMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object);

    private static string ValidBase64(string raw) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

    private static User CreateUnconfirmedUser(string email = "test@example.com")
    {
        var user = UserMethod.Create("testuser", email, "Test", "User").Value;
        user.EmailConfirmed = false;
        user.ModifiedAtUtc = DateTimeOffset.MinValue;
        return user;
    }

    #region Validation & Error Handling

    [Fact(DisplayName = "Validation: Should return InvalidToken when token is not valid Base64")]
    public async Task Handle_ShouldReturnInvalidToken_WhenTokenInvalidBase64()
    {
        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(Guid.NewGuid(), "invalid-base64!!!", null));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidToken.Code);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Validation: Should return InvalidToken when NewEmail is not valid Base64")]
    public async Task Handle_ShouldReturnInvalidToken_WhenNewEmailInvalidBase64()
    {
        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(Guid.NewGuid(), ValidBase64("valid-token"), "invalid-base64!!!"));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidToken.Code);
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Validation: Should return NotFound when user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserNotFound()
    {
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(Guid.NewGuid(), ValidBase64("valid-token"), null));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Validation: Should return NoContent when email already confirmed (idempotent)")]
    public async Task Handle_ShouldReturnNoContent_WhenEmailAlreadyConfirmed()
    {
        var user = CreateUnconfirmedUser();
        user.EmailConfirmed = true;

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), null));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Email Verification (initial — no NewEmail)

    [Fact(DisplayName = "EmailVerification: Should succeed, call ConfirmEmailAsync, set ModifiedAtUtc, send welcome notification")]
    public async Task Handle_ShouldVerifyEmail_WhenNoNewEmail()
    {
        var user = CreateUnconfirmedUser();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), null));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(x => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        user.ModifiedAtUtc.Should().NotBe(DateTimeOffset.MinValue);
        _notificationServiceMock.Verify(x => x.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.WelcomeSent),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "EmailVerification: Should create profile with correct user data")]
    public async Task Handle_ShouldCreateProfile_WithCorrectUserData()
    {
        var user = CreateUnconfirmedUser();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        CreateProfile.Command? captured = null;
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<CreateProfile.Response>>, CancellationToken>((req, _) => captured = (CreateProfile.Command)req)
            .ReturnsAsync(new CreateProfile.Response());

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), null));

        await handler.Handle(command, TestContext.Current.CancellationToken);

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(user.Id);
        captured.Request.FirstName.Should().Be(user.FirstName);
        captured.Request.LastName.Should().Be(user.LastName);
        captured.Request.Email.Should().Be(user.Email);
    }

    [Fact(DisplayName = "EmailVerification: Should return failure when ConfirmEmailAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenConfirmEmailFails()
    {
        var user = CreateUnconfirmedUser();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "ConfirmFailed", Description = "Confirm failed" }));

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), null));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "EmailVerification: Should return failure when UpdateAsync fails after ConfirmEmailAsync")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFailsAfterConfirm()
    {
        var user = CreateUnconfirmedUser();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "UpdateFailed", Description = "Update failed" }));

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), null));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Email Change (with NewEmail)

    [Fact(DisplayName = "EmailChange: Should succeed, call ChangeEmailAsync, set ModifiedAtUtc, NOT send welcome notification")]
    public async Task Handle_ShouldChangeEmail_WhenNewEmailProvided()
    {
        var user = CreateUnconfirmedUser();
        user.Email = "oldemail@test.com";
        var validNewEmail = ValidBase64("newemail@test.com");

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), validNewEmail));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(x => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        user.ModifiedAtUtc.Should().NotBe(DateTimeOffset.MinValue);
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "EmailChange: Should return failure when ChangeEmailAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenChangeEmailFails()
    {
        var user = CreateUnconfirmedUser();
        var validNewEmail = ValidBase64("newemail@test.com");

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "ChangeFailed", Description = "Change failed" }));

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), validNewEmail));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "EmailChange: Should return failure when UpdateAsync fails after ChangeEmailAsync")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFailsAfterChange()
    {
        var user = CreateUnconfirmedUser();
        var validNewEmail = ValidBase64("newemail@test.com");

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "UpdateFailed", Description = "Update failed" }));

        var handler = CreateHandler();
        var command = new ConfirmEmail.Command(
            new ConfirmEmail.Request(user.Id, ValidBase64("valid-token"), validNewEmail));

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _notificationServiceMock.Verify(
            x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<CreateProfile.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
