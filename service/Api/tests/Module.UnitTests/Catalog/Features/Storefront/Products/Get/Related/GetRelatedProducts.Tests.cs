using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Products.Get.Related;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Related;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontRelatedProducts")]
public class GetRelatedProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetRelatedProducts.PagedQueryHandler _handler;

    public GetRelatedProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetRelatedProducts.PagedQueryHandler(_dbContext, NullLogger<GetRelatedProducts.PagedQueryHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return related products sharing taxons")]
    public async Task Handle_ShouldReturnRelated_WhenProductsShareTaxons()
    {
        var taxonomy = new Taxonomy { Name = "Categories" };
        var taxon = new Taxon { Name = "Clothing", Permalink = "clothing", Lft = 1, Rgt = 10, Depth = 0, Taxonomy = taxonomy };

        var product1 = ProductMethod.Create("Main Product", "main", status: ProductStatus.Active).Value;
        product1.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product1.Classifications.Add(new Classification { Product = product1, Taxon = taxon });
        product1.MasterVariantId = VariantMethod.Create(product1.Id, "M", isMaster: true).Value.Id;

        var product2 = ProductMethod.Create("Related Product", "related", status: ProductStatus.Active).Value;
        product2.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product2.Classifications.Add(new Classification { Product = product2, Taxon = taxon });
        product2.MasterVariantId = VariantMethod.Create(product2.Id, "M2", isMaster: true).Value.Id;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Product>().Add(product1);
        _dbContext.Set<Product>().Add(product2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetRelatedProducts.Query(product1.Id, new GetRelatedProducts.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should return empty list when product has no taxons")]
    public async Task Handle_ShouldReturnEmpty_WhenNoSharedTaxons()
    {
        var product = ProductMethod.Create("Lonely Product", "lonely", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetRelatedProducts.Query(product.Id, new GetRelatedProducts.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}
