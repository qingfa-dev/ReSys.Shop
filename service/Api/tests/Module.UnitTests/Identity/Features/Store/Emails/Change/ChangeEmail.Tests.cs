using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Module.Identity.Features.Storefront.Emails.Change;
using Shared.Governance.Conventions;
using Module.UnitTests.Identity.Fixtures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Emails.Change;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "EmailChange")]
public class ChangeEmailTests
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;

    public ChangeEmailTests()
    {
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(Guid.NewGuid());
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _notificationServiceMock = new Mock<INotificationService>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    private ChangeEmail.CommandHandler CreateHandler()
        => new(
            _currentUserMock.Object,
            _userManagerMock.Object,
            _notificationServiceMock.Object,
            Options.Create(new NotificationSetting { ApplicationUrl = "https://example.com" }));

    [Fact(DisplayName = "UseCase: Should return NotFound when user does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "newemail@test.com", Password = "Password1!" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "UseCase: Should return InvalidCredentials when password is incorrect")]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenPasswordIncorrect()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "newemail@test.com", Password = "WrongPassword" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.InvalidCredentials.Code);
    }

    [Fact(DisplayName = "UseCase: Should return EmailDuplicate when new email is already in use")]
    public async Task Handle_ShouldReturnEmailDuplicate_WhenNewEmailAlreadyInUse()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var existingUser = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        existingUser.Id = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser);

        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "existing@test.com", Password = "Password1!" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.EmailDuplicate.Code);
    }

    [Fact(DisplayName = "UseCase: Should succeed when changing to new email")]
    public async Task Handle_ShouldSucceed_WhenChangingToNewEmail()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(x => x.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync("change-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "newemail@test.com", Password = "Password1!" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should send change confirmation email on success")]
    public async Task Handle_ShouldSendConfirmationEmail_WhenEmailChangeSucceeds()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(x => x.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync("change-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "newemail@test.com", Password = "Password1!" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(x => x.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.EmailChanged),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UseCase: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(x => x.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync("change-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Update failed" }));

        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "newemail@test.com", Password = "Password1!" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should succeed when changing to own email (no change)")]
    public async Task Handle_ShouldSucceed_WhenChangingToOwnEmail()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.Email = "current@test.com";

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync("current@test.com"))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync("change-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "current@test.com", Password = "Password1!" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: BuildConfirmPath should create correct URL")]
    public void BuildConfirmPath_ShouldCreateCorrectUrl()
    {
        var userId = Guid.NewGuid();
        var result = ChangeEmail.BuildConfirmPath(userId, "tokenABC", "newemail@test.com");

        result.Should().Be($"confirm-email-change?userId={userId}&token={"tokenABC".ToBase64Url()}&newEmail={"newemail@test.com".ToBase64Url()}");
    }

    [Fact(DisplayName = "UseCase: BuildConfirmPath should base64url encode special characters")]
    public void BuildConfirmPath_ShouldBase64UrlEncodeSpecialCharacters()
    {
        var userId = Guid.NewGuid();
        var result = ChangeEmail.BuildConfirmPath(userId, "token+with=special", "email+test@test.com");

        result.Should().Contain($"token={"token+with=special".ToBase64Url()}");
        result.Should().Contain($"newEmail={"email+test@test.com".ToBase64Url()}");
    }

    [Fact(DisplayName = "UseCase: Should set ModifiedAtUtc when changing email")]
    public async Task Handle_ShouldSetModifiedAtUtc()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.ModifiedAtUtc = DateTimeOffset.MinValue;

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(x => x.GenerateChangeEmailTokenAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync("change-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ChangeEmail.Command(new ChangeEmail.Request { NewEmail = "newemail@test.com", Password = "Password1!" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        user.ModifiedAtUtc.Should().NotBe(DateTimeOffset.MinValue);
    }
}
