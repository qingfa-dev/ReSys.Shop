using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Taxons.Get.All;

namespace Module.UnitTests.Catalog.Features.Storefront.Taxons.Get.All;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontListTaxons")]
public class ListTaxonsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAllTaxons.PagedQueryHandler _handler;

    public ListTaxonsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetAllTaxons.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return flat list of taxons")]
    public async Task Handle_ShouldReturnList()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var taxon = new Taxon { Name = "Clothing", Permalink = "clothing", Lft = 1, Rgt = 2, Depth = 0, Taxonomy = taxonomy };
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAllTaxons.Query(new GetAllTaxons.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Clothing");
    }

    [Fact(DisplayName = "Handler: Should filter by depth")]
    public async Task Handle_ShouldFilterByDepth()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var root = new Taxon { Name = "Root", Permalink = "root", Lft = 1, Rgt = 4, Depth = 0, Taxonomy = taxonomy };
        var child = new Taxon { Name = "Child", Permalink = "root/child", Lft = 2, Rgt = 3, Depth = 1, Parent = root, Taxonomy = taxonomy };
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAllTaxons.Query(new GetAllTaxons.Parameters { Depth = 1 }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Depth.Should().Be(1);
    }
}
