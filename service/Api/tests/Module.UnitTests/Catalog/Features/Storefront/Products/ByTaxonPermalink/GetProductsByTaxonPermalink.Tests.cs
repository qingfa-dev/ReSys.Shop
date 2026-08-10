using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Products.Get.ByTaxonPermalink;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.By.ByTaxonPermalink;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontGetProductsByTaxonPermalink")]
public class GetProductsByTaxonPermalinkTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProductsByTaxonPermalink.PagedQueryHandler _handler;

    public GetProductsByTaxonPermalinkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly, typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProductsByTaxonPermalink.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Taxon> SeedTaxonAsync(string permalink, CancellationToken ct)
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var taxon = new Taxon { Name = permalink, Permalink = permalink, Lft = 1, Rgt = 2, Depth = 0, Taxonomy = taxonomy };
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(ct);
        return taxon;
    }

    private static Product CreateProduct(string name)
    {
        var product = ProductMethod.Create(name, slug: $"{name.ToLowerInvariant()}-{Guid.NewGuid():N}", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        return product;
    }

    [Fact(DisplayName = "Handler: Should return products under the taxon")]
    public async Task Handle_ShouldReturnProducts_UnderTaxon()
    {
        var ct = TestContext.Current.CancellationToken;
        var taxon = await SeedTaxonAsync("clothing", ct);

        var product = CreateProduct("T-Shirt");
        var classification = ClassificationMethod.Create(product.Id, taxon.Id).Value;
        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Classification>().Add(classification);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetProductsByTaxonPermalink.Query("clothing", new GetProductsByTaxonPermalink.Parameters()),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("T-Shirt");
    }

    [Fact(DisplayName = "Handler: Should not return products outside the taxon")]
    public async Task Handle_ShouldExclude_ProductsOutsideTaxon()
    {
        var ct = TestContext.Current.CancellationToken;
        var taxon = await SeedTaxonAsync("shoes", ct);

        var product = CreateProduct("Sneaker");
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetProductsByTaxonPermalink.Query("shoes", new GetProductsByTaxonPermalink.Parameters()),
            ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return NotFound for unknown permalink")]
    public async Task Handle_ShouldNotFind_WhenUnknownTaxon()
    {
        var result = await _handler.Handle(
            new GetProductsByTaxonPermalink.Query("does-not-exist", new GetProductsByTaxonPermalink.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }
}