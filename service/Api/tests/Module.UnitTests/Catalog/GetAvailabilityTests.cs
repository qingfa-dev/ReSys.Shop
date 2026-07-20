using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Get.Availability;
using Module.Inventory.Services;

namespace Module.UnitTests.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class GetAvailabilityTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IStockAvailabilityCalculator> _calc;
    private readonly GetAvailability.QueryHandler _sut;

    public GetAvailabilityTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _db = new ApplicationDbContext(opts);
        _calc = new Mock<IStockAvailabilityCalculator>();
        _sut = new GetAvailability.QueryHandler(_db, _calc.Object);
    }

    public void Dispose() { _db.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "GetAvailability: variant with zero stock returns out_of_stock")]
    public async Task Handle_VariantWithZeroStock_ReturnsOutOfStock()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(variantId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 0 });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("out_of_stock");
    }

    [Fact(DisplayName = "GetAvailability: variant with stock above threshold returns in_stock")]
    public async Task Handle_VariantWithPlentyOfStock_ReturnsInStock()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 10 });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("in_stock");
    }

    [Fact(DisplayName = "GetAvailability: variant with stock at or below threshold returns low_stock")]
    public async Task Handle_VariantWithLowStock_ReturnsLowStock()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 2 });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("low_stock");
    }

    [Fact(DisplayName = "GetAvailability: variant with zero stock but backorderable returns backorderable")]
    public async Task Handle_VariantOutOfStockButBackorderable_ReturnsBackorderable()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 0 });

        _calc.Setup(x => x.GetForVariantAsync(variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(            new StockSnapshot { TotalOnHand = 0, TotalReserved = 0, TotalAvailable = 0, Backorderable = true, Locations = [] });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("backorderable");
    }

    private async Task SeedProductWithVariant(Guid productId, Guid variantId, decimal price)
    {
        var product = new Product { Id = productId, Name = "Test", Slug = "test", IsDeleted = false, AvailableOn = DateTimeOffset.UtcNow };
        var variant = new Variant
        {
            Id = variantId,
            ProductId = productId,
            Product = product,
            IsMaster = false,
            IsDeleted = false,
            Sku = $"SKU-{variantId:N}",
            Position = 1
        };
        product.Variants.Add(variant);
        _db.Set<Product>().Add(product);
        _db.Set<Variant>().Add(variant);
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
