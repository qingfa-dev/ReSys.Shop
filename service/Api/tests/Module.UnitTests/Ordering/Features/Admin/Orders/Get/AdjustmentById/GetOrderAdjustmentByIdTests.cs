using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.AdjustmentById;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.AdjustmentById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderAdjustmentById")]
public class GetOrderAdjustmentByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOrderAdjustmentById.QueryHandler _handler;

    public GetOrderAdjustmentByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOrderAdjustmentById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return adjustment when found")]
    public async Task Handle_ShouldReturnAdjustment_WhenFound()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var adjustment = AdjustmentMethod.Create(
            label: "Discount",
            amount: -5.00m,
            adjustableId: order.Id,
            adjustableType: "Order",
            sourceId: Guid.NewGuid(),
            sourceType: "PromotionAction",
            orderId: order.Id).Value;
        _dbContext.Set<Adjustment>().Add(adjustment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetOrderAdjustmentById.Query(order.Id, adjustment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(adjustment.Id);
        result.Value.Label.Should().Be("Discount");
        result.Value.OrderId.Should().Be(order.Id);
    }

    [Fact(DisplayName = "Handler: Should return not found when adjustment missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetOrderAdjustmentById.Query(order.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(AdjustmentResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
