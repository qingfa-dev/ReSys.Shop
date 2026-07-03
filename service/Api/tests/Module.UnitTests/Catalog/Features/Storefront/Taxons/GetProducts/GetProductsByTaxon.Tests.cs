using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Taxons.Get.Products;

namespace Module.UnitTests.Catalog.Features.Storefront.Taxons.Get.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontProductsByTaxon")]
public class GetProductsByTaxonTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProducts.PagedQueryHandler _handler;

    public GetProductsByTaxonTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProducts.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return products assigned to taxon")]
    public async Task Handle_ShouldReturnProducts_WhenTaxonExists()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var taxon = new Taxon { Name = "Clothing", Permalink = "clothing", Lft = 1, Rgt = 2, Depth = 0, Taxonomy = taxonomy };

        var product = ProductMethod.Create("T-Shirt", "tshirt", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.Classifications.Add(new Classification { Product = product, Taxon = taxon });
        product.MasterVariantId = VariantExtensions.Create(product.Id, "M", isMaster: true).Value.Id;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetProducts.Query(new GetProducts.Parameters { TaxonId = taxon.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should return empty when taxon has no products")]
    public async Task Handle_ShouldReturnEmpty_WhenNoProducts()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var taxon = new Taxon { Name = "Empty", Permalink = "empty", Lft = 1, Rgt = 2, Depth = 0, Taxonomy = taxonomy };
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetProducts.Query(new GetProducts.Parameters { TaxonId = taxon.Id }),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
