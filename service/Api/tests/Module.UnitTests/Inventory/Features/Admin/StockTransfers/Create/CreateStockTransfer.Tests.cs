using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Create;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "CreateStockTransfer")]
public class CreateStockTransferTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateStockTransfer.CommandHandler _handler;
    private readonly Guid _variantA = Guid.NewGuid();
    private readonly Guid _variantB = Guid.NewGuid();
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public CreateStockTransferTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockTransfer).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateStockTransfer.CommandHandler(
            _dbContext,
            new Mock<ILogger<CreateStockTransfer.CommandHandler>>().Object,
            new Mock<ICurrentUser>().Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private CreateStockTransfer.Command CreateCommand(Guid sourceId, Guid destId)
    {
        return new CreateStockTransfer.Command(new CreateStockTransfer.Request
        {
            Reference = "PO-100",
            SourceLocationId = sourceId,
            DestinationLocationId = destId,
            Items =
            [
                new TransferItemRequest { VariantId = _variantA, Quantity = 5 },
                new TransferItemRequest { VariantId = _variantB, Quantity = 3 }
            ]
        });
    }

    [Fact(DisplayName = "Handler: Creates transfer with items populated")]
    public async Task Handle_CreatesTransfer_WithItemsPopulated()
    {
        var result = await _handler.Handle(CreateCommand(_sourceLocationId, _destLocationId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.SourceLocationId.Should().Be(_sourceLocationId);
        result.Value.DestinationLocationId.Should().Be(_destLocationId);
        result.Value.State.Should().Be(TransferState.Draft);
        result.Value.Items.Should().HaveCount(2);

        var first = result.Value.Items.First(i => i.VariantId == _variantA);
        first.Id.Should().NotBe(Guid.Empty);
        first.Quantity.Should().Be(5);
        first.ReceivedQuantity.Should().Be(0);

        var persisted = await _dbContext.Set<StockTransfer>().FirstAsync(t => t.Id == result.Value.Id,
            cancellationToken: TestContext.Current.CancellationToken);
        persisted.TransferItems.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Returns failure when source and destination are the same")]
    public async Task Handle_ReturnsFailure_WhenSameLocation()
    {
        var result = await _handler.Handle(CreateCommand(_sourceLocationId, _sourceLocationId), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
