using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Update;
using Moq;

namespace Module.UnitTests.Shipping.Features.Admin.MethodRates.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "UpdateMethodRateWeight")]
public class UpdateMethodRateWeightValidationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateMethodRate.CommandHandler>> _loggerMock;
    private readonly UpdateMethodRate.CommandHandler _handler;
    private readonly Guid _rateId;

    public UpdateMethodRateWeightValidationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingRate).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateMethodRate.CommandHandler>>();
        _handler = new UpdateMethodRate.CommandHandler(_dbContext, _loggerMock.Object);

        var rate = ShippingRateExtensions.Create(
            name: "Standard",
            cost: 5.99m,
            shipmentId: Guid.NewGuid(),
            shippingMethodId: Guid.NewGuid(),
            deliveryRange: "3-5 days",
            minWeight: 0,
            maxWeight: 2).Value;

        _rateId = rate.Id;
        _dbContext.Set<ShippingRate>().Add(rate);
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update weight fields successfully")]
    public async Task Handle_ShouldUpdateWeightFields()
    {
        var request = new UpdateMethodRate.Request
        {
            MinWeight = 0,
            MaxWeight = 10
        };

        var result = await _handler.Handle(
            new UpdateMethodRate.Command(_rateId, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<ShippingRate>()
            .FirstAsync(r => r.Id == _rateId, TestContext.Current.CancellationToken);
        updated.MinWeight.Should().Be(0);
        updated.MaxWeight.Should().Be(10);
    }

    [Fact(DisplayName = "Handler: Should persist updated weight values")]
    public async Task Handle_ShouldPersistUpdatedWeights()
    {
        await _handler.Handle(
            new UpdateMethodRate.Command(_rateId, new UpdateMethodRate.Request
            {
                MinWeight = 1,
                MaxWeight = 25
            }),
            TestContext.Current.CancellationToken);

        var saved = await _dbContext.Set<ShippingRate>()
            .AsNoTracking()
            .FirstAsync(r => r.Id == _rateId, TestContext.Current.CancellationToken);

        saved.MinWeight.Should().Be(1);
        saved.MaxWeight.Should().Be(25);
    }
}
