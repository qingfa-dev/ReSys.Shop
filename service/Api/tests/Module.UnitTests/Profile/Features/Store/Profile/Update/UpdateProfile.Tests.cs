using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Update;
using Module.UnitTests.Profile.Domain;

using Moq;

namespace Module.UnitTests.Profile.Features.Store.Profile.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileUpdate")]
public class UpdateProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
        _currentUserMock.Setup(x => x.UserName).Returns("test-user");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private UpdateProfile.CommandHandler CreateHandler() => new(_dbContext, _currentUserMock.Object);

    private static UpdateProfile.Request SampleRequest() => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@example.com",
        PhoneNumber = "+9876543210",
        DateOfBirth = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero)
    };

    [Fact(DisplayName = "Handle: Should create new profile when none exists")]
    public async Task Handle_ShouldCreateProfile_WhenNoneExists()
    {
        var request = SampleRequest();
        var command = new UpdateProfile.Command(_userId, request);

        var result = await CreateHandler().Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.PhoneNumber.Should().Be("+9876543210");
    }

    [Fact(DisplayName = "Handle: Should update existing profile when one exists")]
    public async Task Handle_ShouldUpdateProfile_WhenExists()
    {
        var existing = ProfileUserFactory.Create(_userId);
        _dbContext.Set<UserProfile>().Add(existing);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalId = existing.Id;

        var request = SampleRequest();
        var command = new UpdateProfile.Command(_userId, request);

        var result = await CreateHandler().Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(originalId);
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.PhoneNumber.Should().Be("+9876543210");
    }
}
