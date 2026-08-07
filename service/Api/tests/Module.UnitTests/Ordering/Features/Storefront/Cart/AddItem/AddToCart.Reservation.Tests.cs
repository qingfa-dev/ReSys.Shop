using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.AddItem;

using Module.Inventory.Features.Storefront.ReserveCartStock;
using Shared.Application.Systems.SystemInfos;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.AddItem;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "AddToCart.Reservation")]
public class AddToCartReservationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ILogger<AddToCart.CommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ISystemInfo> _systemInfoMock;
    private readonly AddToCart.CommandHandler _handler;

    public AddToCartReservationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly, typeof(StockItem).Assembly, typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _loggerMock = new Mock<ILogger<AddToCart.CommandHandler>>();
        _currentUserMock = new Mock<ICurrentUser>();
        _systemInfoMock = new Mock<ISystemInfo>();
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _handler = new AddToCart.CommandHandler(
            _dbContext, _loggerMock.Object, _currentUserMock.Object, _systemInfoMock.Object, _senderMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "AddToCart: Dispatches ReserveCartStock when stock location exists")]
    public async Task Handle_ShouldDispatchReserveCartStock_WhenStockLocationExists()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var variant = new Variant { Id = variantId, Sku = "SKU-001", Price = 9.99m };
        _dbContext.Set<Variant>().Add(variant);

        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync();

        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserName).Returns("test");
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _senderMock
            .Setup(x => x.Send(
                It.Is<ReserveCartStockCommand>(c => c.LineItems.Any(li => li.VariantId == variantId && li.Quantity == 1)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveCartStockResponse>.Ok(new ReserveCartStockResponse
            {
                ReservationIds = [Guid.NewGuid()]
            }));

        var request = new AddToCart.Request { VariantId = variantId, Quantity = 1 };

        var result = await _handler.Handle(
            new AddToCart.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        _senderMock.Verify(
            x => x.Send(
                It.Is<ReserveCartStockCommand>(c => c.LineItems.Any(li => li.VariantId == variantId && li.Quantity == 1)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "AddToCart: Returns failure when reservation fails")]
    public async Task Handle_ShouldReturnFailure_WhenReservationFails()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var variant = new Variant { Id = variantId, Sku = "SKU-002", Price = 9.99m };
        _dbContext.Set<Variant>().Add(variant);

        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync();

        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 1).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserName).Returns("test");
        _systemInfoMock.Setup(x => x.DefaultCurrency).Returns("USD");

        _senderMock
            .Setup(x => x.Send(
                It.Is<ReserveCartStockCommand>(c => c.LineItems.Any(li => li.VariantId == variantId && li.Quantity == 1)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockReservationResult.Errors.InsufficientStock);

        var request = new AddToCart.Request { VariantId = variantId, Quantity = 1 };

        var result = await _handler.Handle(
            new AddToCart.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }
}
