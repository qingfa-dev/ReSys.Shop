using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Features.Admin.StockMovements.Get.ById;

namespace Module.UnitTests.Inventory.Features.Admin.StockMovements.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetStockMovementById")]
public class GetStockMovementByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockMovementById.QueryHandler _handler;

    public GetStockMovementByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStockMovementById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockMovement> SeedMovement()
    {
        var ct = TestContext.Current.CancellationToken;
        var movement = new StockMovement
        {
            StockItemId = Guid.NewGuid(),
            Quantity = 5,
            PreviousCountOnHand = 10,
            OriginatorType = "Adjustment",
            Reason = "cycle count",
            Action = "adjust"
        };
        _dbContext.Set<StockMovement>().Add(movement);
        await _dbContext.SaveChangesAsync(ct);
        return movement;
    }

    [Fact(DisplayName = "Handle: Returns movement DTO when found")]
    public async Task Handle_ReturnsDto_WhenFound()
    {
        var movement = await SeedMovement();

        var result = await _handler.Handle(new GetStockMovementById.Query(movement.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(movement.Id);
        result.Value.Quantity.Should().Be(5);
        result.Value.PreviousCountOnHand.Should().Be(10);
        result.Value.OriginatorType.Should().Be("Adjustment");
        result.Value.Reason.Should().Be("cycle count");
    }

    [Fact(DisplayName = "Handle: Returns failure when movement does not exist")]
    public async Task Handle_ReturnsFailure_WhenMissing()
    {
        var result = await _handler.Handle(new GetStockMovementById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
