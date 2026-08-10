using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Classifications.Taxons.GetByPermalink;

namespace Module.UnitTests.Catalog.Features.Storefront.Taxons.GetByPermalink;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontGetTaxonByPermalink")]
public class GetTaxonByPermalinkTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetTaxonByPermalink.QueryHandler _handler;

    public GetTaxonByPermalinkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetTaxonByPermalink.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return taxon by permalink with breadcrumb")]
    public async Task Handle_ShouldReturnTaxon_WithBreadcrumb()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var parent = new Taxon { Name = "Clothing", Permalink = "clothing", Lft = 1, Rgt = 4, Depth = 0, Taxonomy = taxonomy };
        var child = new Taxon { Name = "Shirts", Permalink = "shirts", Lft = 2, Rgt = 3, Depth = 1, Parent = parent, Taxonomy = taxonomy };
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, child);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetTaxonByPermalink.Query("shirts"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Shirts");
        result.Value.Permalink.Should().Be("shirts");
        result.Value.Breadcrumb.Should().HaveCount(2);
        result.Value.Breadcrumb.First().Permalink.Should().Be("clothing");
    }

    [Fact(DisplayName = "Handler: Should return direct children")]
    public async Task Handle_ShouldReturnChildren()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var parent = new Taxon { Name = "Clothing", Permalink = "clothing", Lft = 1, Rgt = 4, Depth = 0, Taxonomy = taxonomy };
        var childA = new Taxon { Name = "Shirts", Permalink = "shirts", Lft = 2, Rgt = 3, Depth = 1, Parent = parent, Taxonomy = taxonomy };
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, childA);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetTaxonByPermalink.Query("clothing"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Children.Should().HaveCount(1);
        result.Value.Children.First().Permalink.Should().Be("shirts");
    }

    [Fact(DisplayName = "Handler: Should return failure for unknown permalink")]
    public async Task Handle_ShouldFail_WhenUnknownPermalink()
    {
        var result = await _handler.Handle(
            new GetTaxonByPermalink.Query("does-not-exist"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}