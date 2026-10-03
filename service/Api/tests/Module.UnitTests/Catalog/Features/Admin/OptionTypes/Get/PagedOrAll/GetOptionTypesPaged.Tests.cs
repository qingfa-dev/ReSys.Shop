using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Get.PagedOrAll;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Get.PagedOrAll;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypePaged")]
public class GetOptionTypesPagedOrAllTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOptionTypesPagedOrAll.PagedQueryHandler _handler;

    public GetOptionTypesPagedOrAllTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOptionTypesPagedOrAll.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return paged option types")]
    public async Task Handle_ShouldReturnPagedResults()
    {
        // Arrange
        _dbContext.Set<OptionType>().Add(new OptionType { Name = "T1", Presentation = "P1" });
        _dbContext.Set<OptionType>().Add(new OptionType { Name = "T2", Presentation = "P2" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new QueryingParameters { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(new GetOptionTypesPagedOrAll.Query(parameters), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Handler: Should filter option types by name")]
    public async Task Handle_ShouldFilterByName()
    {
        // Arrange
        _dbContext.Set<OptionType>().Add(new OptionType { Name = "Match", Presentation = "P1" });
        _dbContext.Set<OptionType>().Add(new OptionType { Name = "Other", Presentation = "P2" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new QueryingParameters { Search = "Match", SearchFields = ["Name"] };

        // Act
        var result = await _handler.Handle(new GetOptionTypesPagedOrAll.Query(parameters), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Match");
    }
}
