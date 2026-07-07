using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Create;
using Moq;

namespace Module.UnitTests.Shipping.Features.Admin.MethodRates.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CreateMethodRateWithWeight")]
public class CreateMethodRateWithWeightTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<CreateMethodRate.CommandHandler>> _loggerMock;
    private readonly CreateMethodRate.CommandHandler _handler;
    private readonly Guid _shippingMethodId = Guid.NewGuid();

    public CreateMethodRateWithWeightTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingRate).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateMethodRate.CommandHandler>>();
        _handler = new CreateMethodRate.CommandHandler(_dbContext, _loggerMock.Object);

        _dbContext.Set<ShippingMethod>().Add(new ShippingMethod
        {
            Id = _shippingMethodId,
            Name = "Express Delivery",
            CalculatorType = "FlatPercentItemTotal"
        });
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create rate with weight bounds")]
    public async Task Handle_ShouldCreateRateWithWeightBounds()
    {
        var request = new CreateMethodRate.Request
        {
            Name = "Standard",
            Cost = 5.99m,
            MinWeight = 0,
            MaxWeight = 5
        };

        var result = await _handler.Handle(
            new CreateMethodRate.Command(_shippingMethodId, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Standard");

        var saved = await _dbContext.Set<ShippingRate>()
            .FirstAsync(r => r.ShippingMethodId == _shippingMethodId, TestContext.Current.CancellationToken);
        saved.MinWeight.Should().Be(0);
        saved.MaxWeight.Should().Be(5);
    }

    [Fact(DisplayName = "Handler: Should create rate without weight (null defaults)")]
    public async Task Handle_ShouldCreateRateWithoutWeight()
    {
        var request = new CreateMethodRate.Request
        {
            Name = "Express",
            Cost = 12.99m
        };

        var result = await _handler.Handle(
            new CreateMethodRate.Command(_shippingMethodId, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Express");
        result.Value.MinWeight.Should().BeNull();
        result.Value.MaxWeight.Should().BeNull();
    }
}
