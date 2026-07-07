using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Deactivate;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Deactivate;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "DeactivateShippingMethod")]
public class DeactivateShippingMethodHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DeactivateShippingMethod.CommandHandler>> _loggerMock;
    private readonly DeactivateShippingMethod.CommandHandler _handler;

    public DeactivateShippingMethodHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeactivateShippingMethod.CommandHandler>>();
        _handler = new DeactivateShippingMethod.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should deactivate method when found")]
    public async Task Handle_ShouldDeactivateMethod_WhenFound()
    {
        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        method.AvailableToUsers = true;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeactivateShippingMethod.Command(method.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var saved = await _dbContext.Set<ShippingMethod>()
            .FirstAsync(m => m.Id == method.Id, TestContext.Current.CancellationToken);
        saved.AvailableToUsers.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new DeactivateShippingMethod.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("ShippingMethod.NotFound");
    }
}