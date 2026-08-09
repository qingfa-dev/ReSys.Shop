using Module.Customer.Domain;
using Module.Customer.Features.Storefront.Profiles.Delete;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Store.Profile.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileDelete")]
public class DeleteProfileTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteProfile.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteProfileTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);

        _handler = new DeleteProfile.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    // Check: Resolve UserProfile from persistence store
    // Update: Soft-deactivate profile by setting IsActive=false and recording ModifiedAtUtc
    [Fact(DisplayName = "Handle: Should deactivate profile successfully")]
    public async Task Handle_ShouldDeactivateProfileSuccessfully()
    {
        var profile = ProfileUserFactory.Create(_userId);
        profile.IsActive = true;
        profile.ModifiedAtUtc = null;
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteProfile.Command(_userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updatedProfile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile.Should().NotBeNull();
        updatedProfile.IsActive.Should().BeFalse();
        updatedProfile.ModifiedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle: Should succeed when deleting an already inactive profile")]
    public async Task Handle_ShouldSucceed_WhenAlreadyInactive()
    {
        var profile = ProfileUserFactory.Create(_userId);
        profile.IsActive = false;
        _dbContext.Set<UserProfile>().Add(profile);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteProfile.Command(_userId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updatedProfile = await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == _userId, TestContext.Current.CancellationToken);
        updatedProfile!.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Handle: Should return NotFound when profile does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenProfileMissing()
    {
        var result = await _handler.Handle(new DeleteProfile.Command(_userId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(UserProfileResult.Failure.NotFound.Code);
    }
}
