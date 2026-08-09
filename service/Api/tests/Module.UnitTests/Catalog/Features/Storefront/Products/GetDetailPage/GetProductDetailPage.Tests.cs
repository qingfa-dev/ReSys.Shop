using Microsoft.Extensions.Logging.Abstractions;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Features.Storefront.Products.Get.Detail;
using Module.Inventory.Services;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.GetDetailPage;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontProductDetailPage")]
public class GetProductDetailPageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStockAvailabilityCalculator> _calculatorMock;
    private readonly GetProductDetail.QueryHandler _handler;

    public GetProductDetailPageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _calculatorMock = new Mock<IStockAvailabilityCalculator>();
        _calculatorMock.Setup(x => x.GetAvailableByVariantAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());
        _calculatorMock.Setup(x => x.GetBackorderableByVariantAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());
        _handler = new GetProductDetail.QueryHandler(_dbContext, NullLogger<GetProductDetail.QueryHandler>.Instance, _calculatorMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return product detail page when ID exists")]
    public async Task Handle_ShouldReturnSuccess_WhenIdExists()
    {
        var product = ProductMethod.Create(name: "Test Product", slug: "test-product", description: "A test product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        var variant = VariantMethod.Create(product.Id, "SKU-001", isMaster: true).Value;
        variant.Prices.Add(new Price { Amount = 29.99m, Currency = "USD" });
        product.Variants.Add(variant);
        product.MasterVariantId = variant.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductDetail.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Slug.Should().Be("test-product");
        result.Value.Name.Should().Be("Test Product");
    }

    [Fact(DisplayName = "Handler: Should return failure when ID not found")]
    public async Task Handle_ShouldReturnFailure_WhenIdNotFound()
    {
        var result = await _handler.Handle(new GetProductDetail.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when product is not yet available")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotYetAvailable()
    {
        var product = ProductMethod.Create("Future Product", "future-product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(30);
        product.MasterVariantId = VariantMethod.Create(product.Id, "SKU-FUTURE", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductDetail.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should include variant prices in response")]
    public async Task Handle_ShouldIncludeVariantPrices()
    {
        var product = ProductMethod.Create("Priced Product", "priced-product", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        var master = VariantMethod.Create(product.Id, "MASTER", isMaster: true).Value;
        master.Prices.Add(new Price { Amount = 49.99m, Currency = "USD" });
        product.Variants.Add(master);
        product.MasterVariantId = master.Id;
        var variant = VariantMethod.Create(product.Id, "V-001", isMaster: false).Value;
        variant.Prices.Add(new Price { Amount = 39.99m, Currency = "USD" });
        product.Variants.Add(variant);
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetProductDetail.Query(product.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Variants.Should().HaveCount(2);
        result.Value.MasterVariant.Should().NotBeNull();
        result.Value.MasterVariant!.Price.Should().Be(49.99m);
    }
}
