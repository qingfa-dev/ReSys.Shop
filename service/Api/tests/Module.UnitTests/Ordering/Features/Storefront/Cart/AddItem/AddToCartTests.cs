using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.AddItem;

using Shared.Application.Systems.SystemInfos;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AddItem;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AddToCart")]
public class AddToCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStockReservationService> _reservationServiceMock;
    private readonly Mock<IStockItemService> _stockItemMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<AddToCart.CommandHandler>> _loggerMock;
    private readonly Mock<ISystemInfo> _systemInfoMock;
    private readonly AddToCart.CommandHandler _handler;

    public AddToCartTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(StockItem).Assembly,
            typeof(Variant).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _reservationServiceMock = new Mock<IStockReservationService>();
        _reservationServiceMock
            .Setup(x => x.ReserveForVariantAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockReservationMethod.Reserve(
                Guid.NewGuid(), 1, Guid.NewGuid(), null, 15, cartToken: "test"));

        _stockItemMock = new Mock<IStockItemService>();
        _stockItemMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _loggerMock = new Mock<ILogger<AddToCart.CommandHandler>>();

        _systemInfoMock = new Mock<ISystemInfo>();
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _handler = new AddToCart.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object, _systemInfoMock.Object, _stockItemMock.Object, _reservationServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private Variant SeedActiveProductVariant(string sku, decimal? price = 19.99m, bool discontinued = false)
    {
        var product = ProductMethod.Create("Test Product", status: ProductStatus.Active).Value;
        _dbContext.Set<Product>().Add(product);

        var variant = new Variant
        {
            Sku = sku,
            Price = price,
            ProductId = product.Id,
            DiscontinuedOn = discontinued ? DateTimeOffset.UtcNow.AddDays(-1) : null
        };
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.SaveChanges();
        return variant;
    }

    [Fact(DisplayName = "Handler: Should add item to cart")]
    public async Task Handle_ShouldAddItem_WhenVariantExists()
    {
        // Arrange: Seed product, variant and stock
        var variant = SeedActiveProductVariant("TSHIRT-001", 19.99m);

        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variant.Id, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = variant.Id, Quantity = 2 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify cart was created and item added
        var cart = await _dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Status == OrderStatus.Draft,
                cancellationToken: TestContext.Current.CancellationToken);
        cart.Should().NotBeNull();
        cart!.LineItems.Should().HaveCount(1);
        cart.LineItems.First().Quantity.Should().Be(2);
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = Guid.NewGuid(), Quantity = 1 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(LineItemResult.Errors.VariantNotFound(Guid.NewGuid()).Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when variant is discontinued")]
    public async Task Handle_ShouldReturnFailure_WhenVariantDiscontinued()
    {
        var variant = SeedActiveProductVariant("TSHIRT-DISC", 19.99m, discontinued: true);

        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = variant.Id, Quantity = 1 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotPurchasable.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when product is not active")]
    public async Task Handle_ShouldReturnFailure_WhenProductNotActive()
    {
        var product = ProductMethod.Create("Draft Product", status: ProductStatus.Draft).Value;
        _dbContext.Set<Product>().Add(product);
        var variant = new Variant { Sku = "TSHIRT-DRAFT", Price = 19.99m, ProductId = product.Id };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = variant.Id, Quantity = 1 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotPurchasable.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when variant has no price")]
    public async Task Handle_ShouldReturnFailure_WhenVariantHasNoPrice()
    {
        var variant = SeedActiveProductVariant("TSHIRT-NOPRICE", null);

        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = variant.Id, Quantity = 1 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NoDefaultPrice.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when stock is unavailable")]
    public async Task Handle_ShouldReturnFailure_WhenStockUnavailable()
    {
        var variant = SeedActiveProductVariant("TSHIRT-NOSTOCK", 19.99m);
        _stockItemMock
            .Setup(x => x.IsAvailableAsync(variant.Id, 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = variant.Id, Quantity = 1 }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.CartQuantityInvalid.Code);
    }
}
