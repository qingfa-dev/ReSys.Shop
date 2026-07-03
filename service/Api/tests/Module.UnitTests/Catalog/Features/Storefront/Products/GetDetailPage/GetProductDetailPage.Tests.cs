using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Storefront.Products.Get.Detail;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.Detail;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontProductDetailPage")]
public class GetProductDetailPageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetProductDetail.QueryHandler _handler;

    public GetProductDetailPageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetProductDetail.QueryHandler(_dbContext, NullLogger<GetProductDetail.QueryHandler>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return product detail page when slug exists")]
    public async Task Handle_ShouldReturnSuccess_WhenSlugExists()
    {
        var product = ProductMethod.Create("Test Product", "test-product", description: "A test product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        var variant = VariantExtensions.Create(product.Id, "SKU-001", isMaster: true).Value;
        variant.Prices.Add(new Price { Amount = 29.99m, Currency = "USD" });
        product.Variants.Add(variant);
        product.MasterVariantId = variant.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductDetail.Query("test-product"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Slug.Should().Be("test-product");
        result.Value.Name.Should().Be("Test Product");
    }

    [Fact(DisplayName = "Handler: Should return failure when slug not found")]
    public async Task Handle_ShouldReturnFailure_WhenSlugNotFound()
    {
        var result = await _handler.Handle(new GetProductDetail.Query("non-existent"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when product is not yet available")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotYetAvailable()
    {
        var product = ProductMethod.Create("Future Product", "future-product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(30);
        product.MasterVariantId = VariantExtensions.Create(product.Id, "SKU-FUTURE", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductDetail.Query("future-product"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should include variant prices in response")]
    public async Task Handle_ShouldIncludeVariantPrices()
    {
        var product = ProductMethod.Create("Priced Product", "priced-product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        var master = VariantExtensions.Create(product.Id, "MASTER", isMaster: true).Value;
        master.Prices.Add(new Price { Amount = 49.99m, Currency = "USD" });
        product.Variants.Add(master);
        product.MasterVariantId = master.Id;
        var variant = VariantExtensions.Create(product.Id, "V-001", isMaster: false).Value;
        variant.Prices.Add(new Price { Amount = 39.99m, Currency = "USD" });
        product.Variants.Add(variant);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductDetail.Query("priced-product"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Variants.Should().HaveCount(2);
        result.Value.MasterVariant.Should().NotBeNull();
        result.Value.MasterVariant!.Price.Should().Be(49.99m);
    }
}
