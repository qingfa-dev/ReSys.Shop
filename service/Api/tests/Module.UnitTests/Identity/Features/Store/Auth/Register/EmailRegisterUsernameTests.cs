using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Module.Identity.Features.Store.Auth.Register;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;
using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Store.Auth.Register;

public class EmailRegisterUsernameTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EmailRegister.CommandHandler _handler;

    public EmailRegisterUsernameTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var userStore = new UserStore<User, Role, ApplicationDbContext, Guid>(_dbContext);
        var userManager = new UserManager<User>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<User>(),
            null!, null!,
            new UpperInvariantLookupNormalizer(),
            null!, null!, null!);

        var notificationServiceMock = new Mock<INotificationService>();
        notificationServiceMock
            .Setup(s => s.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new EmailRegister.CommandHandler(
            userManager,
            notificationServiceMock.Object,
            Options.Create(new NotificationSetting { ApplicationUrl = "https://example.com" }),
            Mock.Of<ILogger<EmailRegister.CommandHandler>>());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_UserName_Different_Casing()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            UserName = "ExistingUser",
            NormalizedUserName = "EXISTINGUSER",
            EmailConfirmed = true
        };
        await _dbContext.Set<User>().AddAsync(existingUser, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        _dbContext.ChangeTracker.Clear();

        var result = await _handler.Handle(new EmailRegister.Command(
            new EmailRegister.Request(
                Email: "new@example.com",
                UserName: "existinguser",
                Password: "Password123!",
                FirstName: "New")), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserResult.Failure.UsernameDuplicate.Code);
    }
}
