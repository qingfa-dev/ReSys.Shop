using Module.Catalog.Domain.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.AddItem;

using Module.Inventory.Features.Storefront.ReserveCartStock;
using Shared.Application.Systems.SystemInfos;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AddItem;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AddToCartDefaults")]
public class AddToCartDefaultsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<AddToCart.CommandHandler>> _loggerMock;
    private readonly Mock<ISystemInfo> _systemInfoMock;
    private readonly AddToCart.CommandHandler _handler;

    public AddToCartDefaultsTests()
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

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(x => x.Send(
                It.IsAny<IRequest<Result<ReserveCartStockResponse>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveCartStockResponse>.Ok(
                new ReserveCartStockResponse()));

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _loggerMock = new Mock<ILogger<AddToCart.CommandHandler>>();

        _systemInfoMock = new Mock<ISystemInfo>();
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _handler = new AddToCart.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object, _systemInfoMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create cart with configured currency and no default address")]
    public async Task Handle_ShouldCreateCart_WithConfiguredCurrencyAndNoDefaultAddress()
    {
        var variant = new Variant { Sku = "TSHIRT-001", Price = 19.99m };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variant.Id, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new AddToCart.Command(new AddToCart.Request { VariantId = variant.Id, Quantity = 1 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var cart = await _dbContext.Set<Order>()
            .FirstOrDefaultAsync(x => x.Status == OrderStatus.Draft,
                cancellationToken: TestContext.Current.CancellationToken);
        cart.Should().NotBeNull();
        cart!.Currency.Should().Be("USD");
        cart.ShipAddressId.Should().BeNull();
    }
}
