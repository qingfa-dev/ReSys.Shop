using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Products.Get.Collections;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Collections;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontCollectionPage")]
public class GetCollectionPageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCollectionPage.PagedQueryHandler _handler;

    public GetCollectionPageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetCollectionPage.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return products for given season")]
    public async Task Handle_ShouldReturnProductsBySeason()
    {
        var taxonomy = new Taxonomy { Name = "Seasons" };
        var season = new Taxon { Name = "Spring 2025", Permalink = "spring-2025", Lft = 1, Rgt = 2, Depth = 0, Taxonomy = taxonomy };

        var product = ProductMethod.Create("Spring Dress", "spring-dress", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.Classifications.Add(new Classification { Product = product, Taxon = season });
        product.MasterVariantId = VariantExtensions.Create(product.Id, "M", isMaster: true).Value.Id;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetCollectionPage.Query("Spring 2025", new GetCollectionPage.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Spring Dress");
    }

    [Fact(DisplayName = "Handler: Should return empty for unknown season")]
    public async Task Handle_ShouldReturnEmpty_WhenSeasonUnknown()
    {
        var result = await _handler.Handle(
            new GetCollectionPage.Query("Winter 2099", new GetCollectionPage.Parameters()),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
