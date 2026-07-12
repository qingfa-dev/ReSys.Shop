using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Create;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationCreate")]
public class CreateStockLocationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateStockLocation.CommandHandler>> _loggerMock;
    private readonly CreateStockLocation.CommandHandler _handler;

    public CreateStockLocationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<CreateStockLocation.CommandHandler>>();

        _handler = new CreateStockLocation.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create stock location successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var request = new CreateStockLocation.Request
        {
            Name = "Main Warehouse",
            City = "New York",
            Active = true
        };

        // Act
        var result = await _handler.Handle(new CreateStockLocation.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Main Warehouse");

        var persisted = await _dbContext.Set<StockLocation>()
            .FirstOrDefaultAsync(x => x.Name == "Main Warehouse",
                cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.City.Should().Be("New York");
        persisted.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when name is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenNameIsDuplicate()
    {
        // Arrange
        _dbContext.Set<StockLocation>().Add(
            StockLocationMethod.Create("Main Warehouse").Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateStockLocation.Request
        {
            Name = "Main Warehouse",
            City = "Duplicate"
        };

        // Act
        var result = await _handler.Handle(new CreateStockLocation.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Failure.DuplicateName.Code);
    }
}
