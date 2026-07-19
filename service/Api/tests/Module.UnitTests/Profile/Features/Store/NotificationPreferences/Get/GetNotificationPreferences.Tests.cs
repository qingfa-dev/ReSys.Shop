using Module.Profile.Domain;
using Module.Profile.Features.Store.NotificationPreferences.Get;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Store.NotificationPreferences.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "NotificationPreferencesGet")]
public class GetNotificationPreferencesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetNotificationPreferences.QueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetNotificationPreferencesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetNotificationPreferences.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return notification preferences")]
    public async Task Handle_ShouldReturnPreferences()
    {
        var profile = ProfileUserFactory.Create(_userId);
        profile.Notifications = Module.Profile.Domain.Notifications.NotificationPreferencesMethod.Create(
            enableSms: true, enableEmail: false, enableNewsfeeds: true).Value;
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetNotificationPreferences.Query(_userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.EnableSms.Should().BeTrue();
        result.Value.EnableEmail.Should().BeFalse();
        result.Value.EnableNewsfeeds.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return NotFound when profile does not exist")]
    public async Task Handle_ShouldFail_WhenProfileNotFound()
    {
        var result = await _handler.Handle(new GetNotificationPreferences.Query(_userId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.NotFound.Code);
    }
}
