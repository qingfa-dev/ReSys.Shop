using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.LineItemById;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.LineItemById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderLineItemById")]
public class GetOrderLineItemByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOrderLineItemById.QueryHandler _handler;

    public GetOrderLineItemByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOrderLineItemById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return line item when found")]
    public async Task Handle_ShouldReturnLineItem_WhenFound()
    {
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var lineItem = LineItemMethod.Create(order.Id, Guid.NewGuid(), 2, 29.99m).Value;
        _dbContext.Set<LineItem>().Add(lineItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetOrderLineItemById.Query(order.Id, lineItem.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(lineItem.Id);
        result.Value.Quantity.Should().Be(2);
        result.Value.Price.Should().Be(29.99m);
    }

    [Fact(DisplayName = "Handler: Should return not found when line item missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetOrderLineItemById.Query(order.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(LineItemResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
