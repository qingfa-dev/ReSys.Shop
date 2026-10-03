using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.Create;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemCreate")]
public class CreateStockItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateStockItem.CommandHandler>> _loggerMock;
    private readonly CreateStockItem.CommandHandler _handler;

    public CreateStockItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<CreateStockItem.CommandHandler>>();

        _handler = new CreateStockItem.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create stock item successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var request = new CreateStockItem.Request
        {
            VariantId = variantId,
            StockLocationId = locationId,
            CountOnHand = 10,
            Backorderable = true
        };

        // Act
        var result = await _handler.Handle(new CreateStockItem.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.VariantId.Should().Be(variantId);
        result.Value.StockLocationId.Should().Be(locationId);
        result.Value.CountOnHand.Should().Be(10);

        var persisted = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(x => x.VariantId == variantId && x.StockLocationId == locationId,
                cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.CountOnHand.Should().Be(10);
    }

    [Fact(DisplayName = "Handler: Should return failure when duplicate exists")]
    public async Task Handle_ShouldReturnFailure_WhenDuplicate()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        _dbContext.Set<StockItem>().Add(
            StockItemMethod.Create(locationId, variantId).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateStockItem.Request
        {
            VariantId = variantId,
            StockLocationId = locationId,
            CountOnHand = 5
        };

        // Act
        var result = await _handler.Handle(new CreateStockItem.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockItemResult.Errors.AlreadyExists(variantId, locationId).Code);
    }
}
