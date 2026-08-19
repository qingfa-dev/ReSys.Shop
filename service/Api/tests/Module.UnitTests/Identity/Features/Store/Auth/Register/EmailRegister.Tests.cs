using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Module.Identity.Features.Shared.Storefront.Auth.Register;
using Module.UnitTests.Identity.Fixtures;

using Shared.Governance.Conventions;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Register;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Registration")]
public class EmailRegisterTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IOptions<NotificationSetting> _notificationOptions;
    private readonly ILogger<EmailRegister.CommandHandler> _logger;

    public EmailRegisterTests()
    {
        _userManagerMock = IdentityMocks.CreateUserManagerMock<User>();
        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(s => s.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _notificationOptions = Options.Create(new NotificationSetting { ApplicationUrl = "https://example.com" });
        _logger = Mock.Of<ILogger<EmailRegister.CommandHandler>>();
    }

    private EmailRegister.CommandHandler CreateHandler() => new(
        _userManagerMock.Object,
        _notificationServiceMock.Object,
        _notificationOptions,
        _logger);

    private static EmailRegister.Command CreateCommand(
        string email = "test@example.com",
        string userName = "johndoe",
        string password = "Password1!",
        string firstName = "John",
        string? lastName = null,
        string? phone = null) => new(
        new EmailRegister.Request { Email = email, UserName = userName, Password = password, FirstName = firstName, LastName = lastName, Phone = phone });

    private void SetUpEmailNotTaken() =>
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    private void SetUpUsernameNotTaken() =>
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    private void SetUpCreateSuccess() =>
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

    private void SetUpCreateFailure() =>
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Failed", Description = "Creation failed" }));

    private void SetUpAddRoleSuccess() =>
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

    private void SetUpAddRoleFailure() =>
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleAddFailed", Description = "Failed to add role" }));

    private void SetUpGenerateToken(string token = "token") =>
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(token);

    private void SetUpUpdateSuccess() =>
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

    private void SetUpUpdateFailure() =>
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "UpdateFailed", Description = "Update failed" }));

    private void SetUpFullSuccess()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateSuccess();
        SetUpAddRoleSuccess();
        SetUpGenerateToken();
        SetUpUpdateSuccess();
    }

    // ==================== EMAIL DUPLICATE ====================

    [Fact(DisplayName = "Should return EmailDuplicate when email already exists")]
    public async Task Handle_ShouldReturnEmailDuplicate_WhenEmailAlreadyExists()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.EmailDuplicate.Code);
    }

    // ==================== FIRST NAME REQUIRED ====================

    [Fact(DisplayName = "Should return FirstNameRequired when first name is empty")]
    public async Task Handle_ShouldReturnFirstNameRequired_WhenFirstNameIsEmpty()
    {
        SetUpEmailNotTaken();

        var result = await CreateHandler().Handle(CreateCommand(firstName: ""), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.FirstNameRequired.Code);
    }

    // ==================== USERNAME DUPLICATE ====================

    [Fact(DisplayName = "Should return UsernameDuplicate when username already exists")]
    public async Task Handle_ShouldReturnUsernameDuplicate_WhenUsernameAlreadyExists()
    {
        SetUpEmailNotTaken();
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.UsernameDuplicate.Code);
    }

    // ==================== CREATE FAILURE ====================

    [Fact(DisplayName = "Should return failure when CreateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenCreateAsyncFails()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateFailure();

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== ROLE ASSIGNMENT FAILURE ====================

    [Fact(DisplayName = "Should return failure when AddToRoleAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenAddToRoleAsyncFails()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateSuccess();
        SetUpAddRoleFailure();

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== UPDATE FAILURE ====================

    [Fact(DisplayName = "Should return failure when UpdateAsync fails")]
    public async Task Handle_ShouldReturnFailure_WhenUpdateAsyncFails()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateSuccess();
        SetUpAddRoleSuccess();
        SetUpGenerateToken();
        SetUpUpdateFailure();

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    // ==================== SUCCESSFUL REGISTRATION ====================

    [Fact(DisplayName = "Should return success when registration is successful")]
    public async Task Handle_ShouldReturnSuccess_WhenRegistrationSuccessful()
    {
        SetUpFullSuccess();

        var result = await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact(DisplayName = "Should add user to default role when registration is successful")]
    public async Task Handle_ShouldAddUserToDefaultRole_WhenRegistrationSuccessful()
    {
        SetUpFullSuccess();

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), RoleConstant.Defaults.User), Times.Once);
    }

    [Fact(DisplayName = "Should set user properties correctly on registration")]
    public async Task Handle_ShouldSetUserPropertiesCorrectly()
    {
        User? capturedUser = null;
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((u, _) => capturedUser = u);
        SetUpAddRoleSuccess();
        SetUpGenerateToken();
        SetUpUpdateSuccess();
        await CreateHandler().Handle(CreateCommand(
            email: "Test@Example.COM",
            userName: "johndoe",
            firstName: "John",
            lastName: "Doe",
            phone: "+1234567890"), TestContext.Current.CancellationToken);

        capturedUser.Should().NotBeNull();
        capturedUser.Email.Should().Be("test@example.com");
        capturedUser.UserName.Should().Be("johndoe");
        capturedUser.FirstName.Should().Be("John");
        capturedUser.LastName.Should().Be("Doe");
        capturedUser.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact(DisplayName = "Should lowercase email and username")]
    public async Task Handle_ShouldLowercaseEmailAndUsername()
    {
        User? capturedUser = null;
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((u, _) => capturedUser = u);
        SetUpAddRoleSuccess();
        SetUpGenerateToken();
        SetUpUpdateSuccess();

        await CreateHandler().Handle(CreateCommand(email: "Test@Example.COM", userName: "JohnDoe"),
            TestContext.Current.CancellationToken);

        capturedUser!.Email.Should().Be("test@example.com");
        capturedUser.UserName.Should().Be("johndoe");
    }

    [Fact(DisplayName = "Should set default values on user entity")]
    public async Task Handle_ShouldSetDefaultValues()
    {
        User? capturedUser = null;
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((u, _) => capturedUser = u);
        SetUpAddRoleSuccess();
        SetUpGenerateToken();
        SetUpUpdateSuccess();

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        capturedUser!.IsActive.Should().Be(UserConstant.Defaults.IsActive);
        capturedUser.EmailConfirmed.Should().Be(UserConstant.Defaults.EmailConfirmed);
        capturedUser.CreatedAtUtc.Should().NotBe(default);
    }

    // ==================== NOTIFICATION SENT ====================

    [Fact(DisplayName = "Should send email verification notification on successful registration")]
    public async Task Handle_ShouldSendNotification_WhenRegistrationSuccessful()
    {
        SetUpFullSuccess();

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(s => s.SendAsync(
            It.Is<NotificationMessage>(m => m.UseCase == NotificationUseCase.EmailVerificationRequested),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should not send notification when email already exists")]
    public async Task Handle_ShouldNotSendNotification_WhenEmailDuplicate()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(UserMethod.Create("testuser", "test@example.com", "Test", "User").Value);

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(s => s.SendAsync(
            It.IsAny<NotificationMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should not send notification when create fails")]
    public async Task Handle_ShouldNotSendNotification_WhenCreateFails()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateFailure();

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(s => s.SendAsync(
            It.IsAny<NotificationMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should not send notification when role assignment fails")]
    public async Task Handle_ShouldNotSendNotification_WhenRoleAssignmentFails()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateSuccess();
        SetUpAddRoleFailure();

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(s => s.SendAsync(
            It.IsAny<NotificationMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Should not send notification when update fails")]
    public async Task Handle_ShouldNotSendNotification_WhenUpdateFails()
    {
        SetUpEmailNotTaken();
        SetUpUsernameNotTaken();
        SetUpCreateSuccess();
        SetUpAddRoleSuccess();
        SetUpGenerateToken();
        SetUpUpdateFailure();

        await CreateHandler().Handle(CreateCommand(), TestContext.Current.CancellationToken);

        _notificationServiceMock.Verify(s => s.SendAsync(
            It.IsAny<NotificationMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ==================== VERIFICATION PATH ====================

    [Fact(DisplayName = "BuildVerificationPath should create correct URL")]
    public void BuildVerificationPath_ShouldCreateCorrectUrl()
    {
        var userId = Guid.NewGuid();
        var result = EmailRegister.CommandHandler.BuildVerificationPath(userId, "tokenABC");

        result.Should().Be($"verify-email?userId={userId}&token={"tokenABC".ToBase64Url()}");
    }

    [Fact(DisplayName = "BuildVerificationPath should URL encode special characters")]
    public void BuildVerificationPath_ShouldUrlEncodeSpecialCharacters()
    {
        var userId = Guid.NewGuid();
        var result = EmailRegister.CommandHandler.BuildVerificationPath(userId, "token+with=special&chars");

        result.Should().Be($"verify-email?userId={userId}&token={"token+with=special&chars".ToBase64Url()}");
    }
}
