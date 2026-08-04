using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Module.Identity.Features.Storefront.Emails.Resend;
using Module.UnitTests.Identity.Fixtures;

using Shared.Governance.Conventions;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Emails.Resend;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "EmailResend")]
public class ResendEmailVerificationTests
{
    private readonly Mock<ISystemDateTime> _systemDateTimeMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;

    public ResendEmailVerificationTests()
    {
        _systemDateTimeMock = new Mock<ISystemDateTime>();
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    private ResendEmailVerification.CommandHandler CreateHandler()
        => new(
            _systemDateTimeMock.Object,
            _userManagerMock.Object,
            _notificationServiceMock.Object,
            Options.Create(new NotificationSetting { ApplicationUrl = "https://example.com" }));

    [Fact(DisplayName = "UseCase: Should return NoContent when user does not exist (security)")]
    public async Task Handle_ShouldReturnNoContent_WhenUserNotFound()
    {
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "nonexistent@test.com" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should return NoContent when email already confirmed")]
    public async Task Handle_ShouldReturnNoContent_WhenEmailAlreadyConfirmed()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = true;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "confirmed@test.com" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should succeed when resending verification email")]
    public async Task Handle_ShouldSucceed_WhenResendingVerificationEmail()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = false;

        _systemDateTimeMock
            .Setup(x => x.UtcNow)
            .Returns(DateTimeOffset.UtcNow);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("verification-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "unverified@test.com" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = false;

        _systemDateTimeMock
            .Setup(x => x.UtcNow)
            .Returns(DateTimeOffset.UtcNow);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("verification-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Update failed" }));

        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "unverified@test.com" });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "UseCase: Should set ModifiedAtUtc when resending")]
    public async Task Handle_ShouldSetModifiedAtUtc()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = false;
        user.ModifiedAtUtc = DateTimeOffset.MinValue;

        var expectedTime = DateTimeOffset.UtcNow;
        _systemDateTimeMock
            .Setup(x => x.UtcNow)
            .Returns(expectedTime);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("verification-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "unverified@test.com" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        user.ModifiedAtUtc.Should().Be(expectedTime);
    }

    [Fact(DisplayName = "UseCase: Should set ModifiedBy to System")]
    public async Task Handle_ShouldSetModifiedByToSystem()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = false;
        user.ModifiedBy = null;

        _systemDateTimeMock
            .Setup(x => x.UtcNow)
            .Returns(DateTimeOffset.UtcNow);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("verification-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "unverified@test.com" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        user.ModifiedBy.Should().Be("System");
    }

    [Fact(DisplayName = "UseCase: Should generate verification token")]
    public async Task Handle_ShouldGenerateVerificationToken()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = false;

        _systemDateTimeMock
            .Setup(x => x.UtcNow)
            .Returns(DateTimeOffset.UtcNow);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("new-verification-token")
            .Verifiable();
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);


        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "unverified@test.com" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact(DisplayName = "UseCase: Should send verification email on success")]
    public async Task Handle_ShouldSendVerificationEmail_WhenResending()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        user.EmailConfirmed = false;

        _systemDateTimeMock
            .Setup(x => x.UtcNow)
            .Returns(DateTimeOffset.UtcNow);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("verification-token");
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = CreateHandler();
        var command = new ResendEmailVerification.Command(new ResendEmailVerification.Request { Email = "unverified@test.com" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(x => x.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.EmailVerificationRequested),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UseCase: BuildVerificationPath should create correct URL")]
    public void BuildVerificationPath_ShouldCreateCorrectUrl()
    {
        var userId = Guid.NewGuid();
        var result = ResendEmailVerification.BuildVerificationPath(userId, "tokenABC");

        result.Should().Be($"verify-email?userId={userId}&token={"tokenABC".ToBase64Url()}");

    }

    [Fact(DisplayName = "UseCase: BuildVerificationPath should URL encode special characters")]
    public void BuildVerificationPath_ShouldUrlEncodeSpecialCharacters()
    {
        var userId = Guid.NewGuid();
        var result = ResendEmailVerification.BuildVerificationPath(userId, "token+with=special&chars");

        result.Should().Be($"verify-email?userId={userId}&token={"token+with=special&chars".ToBase64Url()}");

    }

    [Fact(DisplayName = "BugFix: BuildVerificationPath encodes token with base64url, decodable by ConfirmEmail's TryFromBase64Url")]
    public void BuildVerificationPath_EncodesBase64Url_Decodable()
    {
        var userId = Guid.NewGuid();
        var rawToken = "test-token-with/special+chars";

        var path = ResendEmailVerification.BuildVerificationPath(userId, rawToken);

        var tokenFromUrl = ExtractQueryParam(path, "token");
        var success = tokenFromUrl.TryFromBase64Url(out var decoded);
        success.Should().BeTrue();
        decoded.Should().Be(rawToken);
    }

    [Fact(DisplayName = "BugFix: BuildVerificationPath does not contain URL-unsafe characters")]
    public void BuildVerificationPath_NoUnsafeUrlChars()
    {
        var path = ResendEmailVerification.BuildVerificationPath(Guid.NewGuid(), "test");
        var token = ExtractQueryParam(path, "token");

        token.Should().NotContain("+");
        token.Should().NotContain("/");
        token.Should().NotContain("=");
    }

    private static string ExtractQueryParam(string url, string param)
    {
        var query = url[(url.IndexOf('?') + 1)..];
        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts[0] == param) return parts[1];
        }
        return string.Empty;
    }
}
