using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Delete;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "DeleteShippingMethod")]
public class DeleteShippingMethodHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DeleteShippingMethod.CommandHandler>> _loggerMock;
    private readonly DeleteShippingMethod.CommandHandler _handler;

    public DeleteShippingMethodHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeleteShippingMethod.CommandHandler>>();
        _handler = new DeleteShippingMethod.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should soft delete method when found")]
    public async Task Handle_ShouldSoftDeleteMethod_WhenFound()
    {
        var method = ShippingMethodExtensions.Create("Standard", "flat_rate").Value;
        method.IsDeleted = false;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteShippingMethod.Command(method.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var saved = await _dbContext.Set<ShippingMethod>()
            .IgnoreQueryFilters()
            .FirstAsync(m => m.Id == method.Id, TestContext.Current.CancellationToken);
        saved.IsDeleted.Should().BeTrue();
        saved.DeletedAtUtc.Should().NotBeNull();
        saved.DeletedBy.Should().Be("System");
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new DeleteShippingMethod.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("ShippingMethod.NotFound");
    }
}