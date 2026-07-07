using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Create;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CreateShippingMethod")]
public class CreateShippingMethodHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CreateShippingMethod.CommandHandler>> _loggerMock;
    private readonly CreateShippingMethod.CommandHandler _handler;

    public CreateShippingMethodHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateShippingMethod.CommandHandler>>();
        _handler = new CreateShippingMethod.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create method when valid request")]
    public async Task Handle_ShouldCreateMethod_WhenValidRequest()
    {
        var request = new CreateShippingMethod.Request
        {
            Name = "Standard Shipping",
            CalculatorType = "flat_rate",
            TrackingUrl = "https://track.example.com/:tracking",
            AdminName = "Standard",
            Position = 1
        };

        var result = await _handler.Handle(
            new CreateShippingMethod.Command(request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Standard Shipping");

        var saved = await _dbContext.Set<ShippingMethod>()
            .FirstAsync(m => m.Name == "Standard Shipping", TestContext.Current.CancellationToken);
        saved.CalculatorType.Should().Be("flat_rate");
        saved.TrackingUrl.Should().Be("https://track.example.com/:tracking");
    }

    [Fact(DisplayName = "Handler: Should return duplicate when name exists")]
    public async Task Handle_ShouldReturnDuplicate_WhenNameExists()
    {
        var existing = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(existing);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateShippingMethod.Request
        {
            Name = "Standard",
            CalculatorType = "flat_rate"
        };

        var result = await _handler.Handle(
            new CreateShippingMethod.Command(request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("ShippingMethod.Code.Duplicate");
    }
}