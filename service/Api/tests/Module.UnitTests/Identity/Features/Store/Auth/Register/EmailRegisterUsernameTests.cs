using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Module.Identity.Features.Store.Auth.Register;
using Module.UnitTests.Identity.Fixtures;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Register;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Registration")]
public class EmailRegisterUsernameTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IOptions<NotificationSetting> _notificationOptions;
    private readonly ILogger<EmailRegister.CommandHandler> _logger;

    public EmailRegisterUsernameTests()
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

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_UserName_Different_Casing()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync("existinguser"))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = "existing@example.com",
                UserName = "ExistingUser",
                EmailConfirmed = true
            });

        var handler = CreateHandler();
        var result = await handler.Handle(new EmailRegister.Command(
            new EmailRegister.Request(
                Email: "new@example.com",
                UserName: "existinguser",
                Password: "Password123!",
                FirstName: "New")), default);

        result.IsFailure.Should().BeTrue();
    }
}
