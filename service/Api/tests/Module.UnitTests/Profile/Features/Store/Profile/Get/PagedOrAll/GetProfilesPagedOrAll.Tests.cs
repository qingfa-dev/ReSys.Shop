using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Get.PagedOrAll;
using Module.UnitTests.Profile.Domain;

using Shared.Operational.Persistence.Data;

namespace Module.UnitTests.Profile.Features.Store.Profile.Get.PagedOrAll;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileGetAll")]
public class GetProfilesPagedOrAllTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProfilesPagedOrAll.QueryHandler _handler;

    public GetProfilesPagedOrAllTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetProfilesPagedOrAll.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    // Get: Return all profiles from the database with no filtering applied.
    [Fact(DisplayName = "Handle: Should return all profiles when no filter")]
    public async Task Handle_ShouldReturnAllProfiles_WhenNoFilter()
    {
        var profile1 = ProfileUserFactory.Create(Guid.NewGuid());
        var profile2 = ProfileUserFactory.Create(Guid.NewGuid());
        _dbContext.Set<UserProfile>().AddRange(profile1, profile2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProfilesPagedOrAll.Query(new GetProfilesPagedOrAll.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    // Get: Return empty result when no profiles exist in the database.
    [Fact(DisplayName = "Handle: Should return empty list when no profiles")]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProfiles()
    {
        var result = await _handler.Handle(new GetProfilesPagedOrAll.Query(new GetProfilesPagedOrAll.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // Apply: Paginate results respecting Page and PageSize parameters.
    [Fact(DisplayName = "Handle: Should handle pagination correctly")]
    public async Task Handle_ShouldHandlePagination()
    {
        for (int i = 0; i < 5; i++)
        {
            _dbContext.Set<UserProfile>().Add(ProfileUserFactory.Create(Guid.NewGuid()));
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProfilesPagedOrAll.Query(new GetProfilesPagedOrAll.Parameters { PageNumber = 1, PageSize = 2 }), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    // Apply: Return correct second page with pagination.
    [Fact(DisplayName = "Handle: Should return second page correctly")]
    public async Task Handle_ShouldReturnSecondPage()
    {
        for (int i = 0; i < 5; i++)
        {
            _dbContext.Set<UserProfile>().Add(ProfileUserFactory.Create(Guid.NewGuid()));
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProfilesPagedOrAll.Query(new GetProfilesPagedOrAll.Parameters { PageNumber = 2, PageSize = 2 }), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
    }

    // Apply: Return all profiles when page size exceeds total count.
    [Fact(DisplayName = "Handle: Should return all profiles when page size exceeds total")]
    public async Task Handle_ShouldReturnAll_WhenPageSizeExceedsTotal()
    {
        for (int i = 0; i < 3; i++)
        {
            _dbContext.Set<UserProfile>().Add(ProfileUserFactory.Create(Guid.NewGuid()));
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProfilesPagedOrAll.Query(new GetProfilesPagedOrAll.Parameters { PageNumber = 1, PageSize = 10 }), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }
}
