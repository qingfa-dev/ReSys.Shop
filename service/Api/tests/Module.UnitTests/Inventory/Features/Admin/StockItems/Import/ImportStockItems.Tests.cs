using System.Text;

using Microsoft.AspNetCore.Http;

using Module.Inventory.Domain.StockItems;
using Module.Inventory.Features.Admin.StockItems.Import;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Import;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "ImportStockItems")]
public class ImportStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ImportStockItems.CommandHandler _handler;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _variantA = Guid.NewGuid();
    private readonly Guid _variantB = Guid.NewGuid();
    private readonly Guid _stockLocationId = Guid.NewGuid();

    public ImportStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");
        _handler = new ImportStockItems.CommandHandler(
            _dbContext,
            new Mock<ILogger<ImportStockItems.CommandHandler>>().Object,
            _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static ImportStockItems.Command CreateImportCommand(string csv)
    {
        var bytes = Encoding.UTF8.GetBytes(csv);
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "import.csv");
        return new ImportStockItems.Command(new ImportStockItems.Request { File = file });
    }

    private async Task<StockItem> SeedExistingStockItem()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new StockItem
        {
            VariantId = _variantB,
            StockLocationId = _stockLocationId,
            CountOnHand = 1,
            Backorderable = false
        };
        _dbContext.Set<StockItem>().Add(item);
        await _dbContext.SaveChangesAsync(ct);
        return item;
    }

    [Fact(DisplayName = "Handler: Reports created, updated, and failed counts from CSV")]
    public async Task Handle_ReportsCreatedUpdatedFailedCounts()
    {
        await SeedExistingStockItem();

        var csv = string.Join('\n',
            "VariantId,StockLocationId,CountOnHand,Backorderable",
            $"{_variantA},{_stockLocationId},10,true",
            $"{_variantB},{_stockLocationId},99,false",
            $"not-a-guid,{_stockLocationId},5,false");
        var result = await _handler.Handle(CreateImportCommand(csv), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Created.Should().Be(1);
        result.Value.Updated.Should().Be(1);
        result.Value.Failed.Should().Be(1);
        result.Value.Errors.Should().ContainSingle();
    }

    [Fact(DisplayName = "Handler: Creates new stock item and updates existing from CSV")]
    public async Task Handle_CreatesAndUpdatesStockItems()
    {
        var existing = await SeedExistingStockItem();

        var csv = string.Join('\n',
            "VariantId,StockLocationId,CountOnHand,Backorderable",
            $"{_variantA},{_stockLocationId},10,true",
            $"{_variantB},{_stockLocationId},50,false");
        var result = await _handler.Handle(CreateImportCommand(csv), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Created.Should().Be(1);
        result.Value.Updated.Should().Be(1);
        result.Value.Failed.Should().Be(0);

        var created = await _dbContext.Set<StockItem>().FirstAsync(si => si.VariantId == _variantA,
            cancellationToken: TestContext.Current.CancellationToken);
        created.CountOnHand.Should().Be(10);
        created.Backorderable.Should().BeTrue();

        var freshExisting = await _dbContext.Set<StockItem>().FirstAsync(si => si.Id == existing.Id,
            cancellationToken: TestContext.Current.CancellationToken);
        freshExisting.CountOnHand.Should().Be(50);
        freshExisting.Backorderable.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Rejects empty file")]
    public async Task Handle_RejectsEmptyFile()
    {
        var bytes = Array.Empty<byte>();
        var file = new FormFile(new MemoryStream(bytes), 0, 0, "file", "empty.csv");
        var result = await _handler.Handle(
            new ImportStockItems.Command(new ImportStockItems.Request { File = file }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
