using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Update.Adjustment;
using AdjustmentDomain = Module.Ordering.Domain.Adjustments.Adjustment;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Update.Adjustment;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderAdjustment")]
public class UpdateOrderAdjustmentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateOrderAdjustment.CommandHandler _handler;

    public UpdateOrderAdjustmentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateOrderAdjustment.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<(Order Order, AdjustmentDomain Adjustment)> SeedAsync()
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
        _dbContext.Set<AdjustmentDomain>().Add(adjustment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (order, adjustment);
    }

    [Fact(DisplayName = "Handler: Should close adjustment")]
    public async Task Handle_ShouldCloseAdjustment()
    {
        var (order, adjustment) = await SeedAsync();

        var result = await _handler.Handle(
            new UpdateOrderAdjustment.Command(order.Id, adjustment.Id, new UpdateOrderAdjustment.Request
            {
                Action = UpdateOrderAdjustment.AdjustmentAction.Close
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be("closed");
    }

    [Fact(DisplayName = "Handler: Should open closed adjustment")]
    public async Task Handle_ShouldOpenAdjustment()
    {
        var (order, adjustment) = await SeedAsync();
        adjustment.State = "closed";
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateOrderAdjustment.Command(order.Id, adjustment.Id, new UpdateOrderAdjustment.Request
            {
                Action = UpdateOrderAdjustment.AdjustmentAction.Open
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be("open");
    }

    [Fact(DisplayName = "Handler: Should mark adjustment ineligible")]
    public async Task Handle_ShouldMarkIneligible()
    {
        var (order, adjustment) = await SeedAsync();

        var result = await _handler.Handle(
            new UpdateOrderAdjustment.Command(order.Id, adjustment.Id, new UpdateOrderAdjustment.Request
            {
                Action = UpdateOrderAdjustment.AdjustmentAction.MarkIneligible
            }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Eligible.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return not found when adjustment missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new UpdateOrderAdjustment.Command(order.Id, Guid.NewGuid(), new UpdateOrderAdjustment.Request
            {
                Action = UpdateOrderAdjustment.AdjustmentAction.Close
            }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(AdjustmentResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
