using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.StockLocations.Update;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationUpdate")]
public class UpdateStockLocationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<UpdateStockLocation.CommandHandler>> _loggerMock;
    private readonly UpdateStockLocation.CommandHandler _handler;

    public UpdateStockLocationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockLocation).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<UpdateStockLocation.CommandHandler>>();

        _handler = new UpdateStockLocation.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update stock location successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange: Seed entity
        var location = StockLocationMethod.Create("Original Name", city: "NYC").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateStockLocation.Request
        {
            Name = "Updated Name",
            City = "Los Angeles"
        };

        // Act
        var result = await _handler.Handle(
            new UpdateStockLocation.Command(location.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.City.Should().Be("Los Angeles");

        var persisted = await _dbContext.Set<StockLocation>()
            .FirstOrDefaultAsync(x => x.Id == location.Id,
                cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Updated Name");
    }

    [Fact(DisplayName = "Handler: Should return failure when location not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        // Arrange
        var request = new UpdateStockLocation.Request { Name = "Ghost" };

        // Act
        var result = await _handler.Handle(
            new UpdateStockLocation.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0]!.Code.Should().Be(StockLocationResult.Failure.NotFound.Code);
    }
}
