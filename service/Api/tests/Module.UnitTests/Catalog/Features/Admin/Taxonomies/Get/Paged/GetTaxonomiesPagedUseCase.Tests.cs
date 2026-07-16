using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Get.Paged;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyGetPaged")]
public class GetTaxonomiesPagedTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetTaxonomiesPaged.PagedQueryHandler _handler;

    public GetTaxonomiesPagedTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetTaxonomiesPaged.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return paged taxonomies")]
    public async Task Handle_ShouldReturnPagedResult()
    {
        // Arrange
        _dbContext.Set<Taxonomy>().AddRange(
            TaxonomyMethod.Create("Tax 1", "Presentation 1", 0).Value,
            TaxonomyMethod.Create("Tax 2", "Presentation 2", 1).Value,
            TaxonomyMethod.Create("Tax 3", "Presentation 3", 2).Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetTaxonomiesPaged.Parameters
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var result = await _handler.Handle(new GetTaxonomiesPaged.Query(parameters), TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Handler: Should return filtered taxonomies")]
    public async Task Handle_ShouldReturnFilteredResult()
    {
        // Arrange
        _dbContext.Set<Taxonomy>().AddRange(
            TaxonomyMethod.Create("Apple", "Apple", 0).Value,
            TaxonomyMethod.Create("Banana", "Banana", 1).Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetTaxonomiesPaged.Parameters
        {
            Search = "Apple",
            SearchFields = [ nameof(Taxonomy.Name) ]
        };

        // Act
        var result = await _handler.Handle(new GetTaxonomiesPaged.Query(parameters), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("apple");
    }

    [Fact(DisplayName = "Handler: Should return sorted taxonomies")]
    public async Task Handle_ShouldReturnSortedResult()
    {
        // Arrange
        _dbContext.Set<Taxonomy>().AddRange(
            TaxonomyMethod.Create("B", "B", 0).Value,
            TaxonomyMethod.Create("A", "A", 1).Value,
            TaxonomyMethod.Create("C", "C", 2).Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetTaxonomiesPaged.Parameters
        {
            Sort = [ "Name" ]
        };

        // Act
        var result = await _handler.Handle(new GetTaxonomiesPaged.Query(parameters), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Select(x => x.Name).Should().ContainInOrder("a", "b", "c");
    }

    [Fact(DisplayName = "Handler: Should return empty result when no taxonomies exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoTaxonomies()
    {
        var parameters = new GetTaxonomiesPaged.Parameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(new GetTaxonomiesPaged.Query(parameters), TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handler: Should return combined filtered and sorted result")]
    public async Task Handle_ShouldReturnFilteredAndSorted()
    {
        _dbContext.Set<Taxonomy>().AddRange(
            TaxonomyMethod.Create("Lambda", "First", 0).Value,
            TaxonomyMethod.Create("Gamma", "Second", 1).Value,
            TaxonomyMethod.Create("Beta", "Third", 2).Value,
            TaxonomyMethod.Create("Alpha", "Fourth", 3).Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetTaxonomiesPaged.Parameters
        {
            Search = "a",
            SearchFields = [nameof(Taxonomy.Name)],
            Sort = ["Name desc"],
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(new GetTaxonomiesPaged.Query(parameters), TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
        result.Items.Select(x => x.Name).Should().BeInDescendingOrder();
    }

    [Fact(DisplayName = "Handler: Should return all items when page size exceeds total")]
    public async Task Handle_ShouldReturnAll_WhenPageSizeExceedsTotal()
    {
        _dbContext.Set<Taxonomy>().AddRange(
            TaxonomyMethod.Create("A", "A", 0).Value,
            TaxonomyMethod.Create("B", "B", 1).Value,
            TaxonomyMethod.Create("C", "C", 2).Value
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var parameters = new GetTaxonomiesPaged.Parameters
        {
            PageNumber = 1,
            PageSize = 100
        };

        var result = await _handler.Handle(new GetTaxonomiesPaged.Query(parameters), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }
}
