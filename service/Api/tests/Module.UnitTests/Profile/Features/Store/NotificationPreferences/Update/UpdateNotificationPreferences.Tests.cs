using Module.Profile.Domain;
using Module.Profile.Features.Store.NotificationPreferences.Update;
using Module.UnitTests.Identity.Fixtures;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Store.NotificationPreferences.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "NotificationPreferencesUpdate")]
public class UpdateNotificationPreferencesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateNotificationPreferences.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateNotificationPreferencesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        _handler = new UpdateNotificationPreferences.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should update notification preferences")]
    public async Task Handle_ShouldUpdatePreferences()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateNotificationPreferences.Request
        {
            EnableSms = true,
            EnableEmail = false,
            EnableNewsfeeds = true
        };

        var result = await _handler.Handle(new UpdateNotificationPreferences.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.EnableSms.Should().BeTrue();
        result.Value.EnableEmail.Should().BeFalse();
        result.Value.EnableNewsfeeds.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return NotFound when user is not authenticated")]
    public async Task Handle_ShouldFail_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var result = await _handler.Handle(new UpdateNotificationPreferences.Command(new UpdateNotificationPreferences.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var result = await _handler.Handle(new UpdateNotificationPreferences.Command(new UpdateNotificationPreferences.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should turn off all preferences")]
    public async Task Handle_ShouldTurnOffAllPreferences()
    {
        var profile = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateNotificationPreferences.Request
        {
            EnableSms = false,
            EnableEmail = false,
            EnableNewsfeeds = false
        };

        var result = await _handler.Handle(new UpdateNotificationPreferences.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.EnableSms.Should().BeFalse();
        result.Value.EnableEmail.Should().BeFalse();
        result.Value.EnableNewsfeeds.Should().BeFalse();
    }
}
