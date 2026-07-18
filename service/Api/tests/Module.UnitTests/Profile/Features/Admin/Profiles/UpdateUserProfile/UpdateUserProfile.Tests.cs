using Module.Profile.Domain;
using Module.Profile.Features.Admin.Profiles.UpdateUserProfile;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Admin.Profile.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "AdminUpdateUserProfile")]
public class UpdateUserProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateUserProfile.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateUserProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateUserProfile.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static UpdateUserProfile.Request SampleRequest(Guid userId) => new()
    {
        UserId = userId,
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@example.com",
        PhoneNumber = "+9876543210",
        DateOfBirth = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero)
    };

    [Fact(DisplayName = "Handle: Should update existing profile when one exists")]
    public async Task Handle_ShouldUpdateExistingProfile()
    {
        var existing = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(existing);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalId = existing.Id;

        var request = SampleRequest(_userId);
        var command = new UpdateUserProfile.Command(request);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(originalId);
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.PhoneNumber.Should().Be("+9876543210");
    }

    [Fact(DisplayName = "Handle: Should create new profile when none exists (upsert)")]
    public async Task Handle_ShouldCreateProfile_WhenNotExists()
    {
        var request = SampleRequest(_userId);
        var command = new UpdateUserProfile.Command(request);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.PhoneNumber.Should().Be("+9876543210");

        var profile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        profile.Should().NotBeNull();
        profile!.FirstName.Should().Be("Jane");
    }
}
