using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Prices;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Features.Storefront.Products.Get.Detail;
using Module.Inventory.Services;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontProductDetailPage")]
public class GetProductDetailPageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStockItemService> _stockItemMock;
    private readonly GetProductDetail.QueryHandler _handler;

    public GetProductDetailPageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _stockItemMock = new Mock<IStockItemService>();
        _stockItemMock.Setup(x => x.GetStockAvailabilityAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VariantStockAvailability>());
        _handler = new GetProductDetail.QueryHandler(_dbContext, NullLogger<GetProductDetail.QueryHandler>.Instance, _stockItemMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Product> SeedProductWithVariant(Guid variantId, string variantName = "Test Variant")
    {
        var ct = TestContext.Current.CancellationToken;
        var taxon = new Taxon { Name = "Test Category", Permalink = "test-category" };
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(ct);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            AvailableOn = DateTimeOffset.UtcNow.AddDays(-1),
        };
        product.Classifications.Add(new Classification { TaxonId = taxon.Id, ProductId = product.Id });

        var variant = new Variant
        {
            Id = variantId,
            Sku = $"SKU-{variantId.ToString()[..8]}",
            ProductId = product.Id,
        };
        variant.Prices.Add(new Price { Amount = 29.99m, Currency = "USD", VariantId = variantId });
        product.Variants.Add(variant);

        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(ct);
        return product;
    }

    [Fact(DisplayName = "Handler: Should return product detail with stock info")]
    public async Task Handle_ShouldReturnProductDetail_WithStockInfo()
    {
        var ct = TestContext.Current.CancellationToken;
        var variantId = Guid.NewGuid();
        var product = await SeedProductWithVariant(variantId);

        var result = await _handler.Handle(new GetProductDetail.Query(product.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Variants.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should return failure when product not found")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(new GetProductDetail.Query(Guid.NewGuid()), ct);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should enrich variants with stock availability")]
    public async Task Handle_ShouldEnrichVariants_WithStockAvailability()
    {
        var ct = TestContext.Current.CancellationToken;
        var variantId = Guid.NewGuid();
        var product = await SeedProductWithVariant(variantId);

        _stockItemMock.Setup(x => x.GetStockAvailabilityAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(variantId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VariantStockAvailability>
            {
                new() { VariantId = variantId, TotalAvailable = 15, Backorderable = true }
            });

        var result = await _handler.Handle(new GetProductDetail.Query(product.Id), ct);

        result.IsSuccess.Should().BeTrue();
        var variant = result.Value.Variants.Should().ContainSingle().Subject;
        variant.Stock.Should().NotBeNull();
    }
}
