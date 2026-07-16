using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations;
using Module.Ordering.Domain.LineItems;
using Module.Inventory.Features.Storefront.CartReservations.Reserve;
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
    private readonly Mock<ISender> _senderMock;
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

        _senderMock = new Mock<ISender>();
        _senderMock
            .Setup(x => x.Send(
                It.IsAny<IRequest<Result<ReserveCartStock.Response>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveCartStock.Response>.Ok(
                new ReserveCartStock.Response()));

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

    [Fact(DisplayName = "Handler: Should add item to cart")]
    public async Task Handle_ShouldAddItem_WhenVariantExists()
    {
        // Arrange: Seed variant and stock
        var variant = new Variant { Sku = "TSHIRT-001", Price = 19.99m };
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
}
