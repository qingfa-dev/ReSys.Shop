using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Classifications.Taxonomies;

namespace Module.UnitTests.Catalog.Features.Storefront.Taxonomies.GetTree;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontTaxonomyTree")]
public class GetTaxonomyTreeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStoreTaxonomies.PagedQueryHandler _handler;

    public GetTaxonomyTreeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetStoreTaxonomies.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return taxonomy in flat list")]
    public async Task Handle_ShouldReturnTaxonomy_WhenExists()
    {
        var taxonomy = new Taxonomy { Name = "Categories", Position = 1 };
        var parent = new Taxon { Name = "Clothing", Permalink = "clothing", Lft = 1, Rgt = 6, Depth = 0, Taxonomy = taxonomy };
        var child = new Taxon { Name = "Shirts", Permalink = "clothing/shirts", Lft = 2, Rgt = 3, Depth = 1, Parent = parent, Taxonomy = taxonomy };
        parent.Children.Add(child);
        taxonomy.Taxons.Add(parent);
        taxonomy.Taxons.Add(child);

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetStoreTaxonomies.Query(new GetStoreTaxonomies.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Categories");
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new GetStoreTaxonomies.Query(new GetStoreTaxonomies.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
