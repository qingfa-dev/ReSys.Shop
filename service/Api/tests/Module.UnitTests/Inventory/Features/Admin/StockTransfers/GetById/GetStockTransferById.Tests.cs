using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.GetById;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.GetById;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetStockTransferById")]
public class GetStockTransferByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetStockTransferById.QueryHandler _handler;
    private readonly Guid _variantA = Guid.NewGuid();
    private readonly Guid _variantB = Guid.NewGuid();
    private readonly Guid _sourceLocationId = Guid.NewGuid();
    private readonly Guid _destLocationId = Guid.NewGuid();

    public GetStockTransferByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockTransfer).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetStockTransferById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<StockTransfer> SeedTransfer()
    {
        var ct = TestContext.Current.CancellationToken;
        var createResult = StockTransferExtensions.Create(
            "TR-REF", _sourceLocationId, _destLocationId,
            [(_variantA, 5), (_variantB, 3)]);
        var transfer = createResult.Value;
        _dbContext.Set<StockTransfer>().Add(transfer);
        await _dbContext.SaveChangesAsync(ct);
        return transfer;
    }

    [Fact(DisplayName = "Handle: Returns transfer with items populated")]
    public async Task Handle_ReturnsTransfer_WithItemsPopulated()
    {
        var transfer = await SeedTransfer();

        var result = await _handler.Handle(new GetStockTransferById.Query(transfer.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(transfer.Id);
        result.Value.Reference.Should().Be("TR-REF");
        result.Value.SourceLocationId.Should().Be(_sourceLocationId);
        result.Value.DestinationLocationId.Should().Be(_destLocationId);
        result.Value.Items.Should().HaveCount(2);

        var first = result.Value.Items.First(i => i.VariantId == _variantA);
        first.Id.Should().NotBe(Guid.Empty);
        first.VariantId.Should().Be(_variantA);
        first.Quantity.Should().Be(5);
        first.ReceivedQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Returns not-found when transfer does not exist")]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new GetStockTransferById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
