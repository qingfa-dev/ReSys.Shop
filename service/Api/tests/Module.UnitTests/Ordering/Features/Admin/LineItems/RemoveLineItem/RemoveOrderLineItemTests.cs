using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.LineItems.RemoveLineItem;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.LineItems.RemoveLineItem;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "RemoveOrderLineItem")]
public class RemoveOrderLineItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RemoveOrderLineItem.CommandHandler _handler;

    public RemoveOrderLineItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new RemoveOrderLineItem.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should remove a line item from a draft order")]
    public async Task Handle_ShouldRemoveLineItem_FromDraftOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var (order, lineItem) = await SeedOrderWithLineItem();

        var result = await _handler.Handle(
            new RemoveOrderLineItem.Command(order.Id, lineItem.Id),
            ct);

        result.IsSuccess.Should().BeTrue();

        var lineItems = await _dbContext.Set<LineItem>().Where(li => li.OrderId == order.Id).ToListAsync(ct);
        lineItems.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return not found when line item missing")]
    public async Task Handle_ShouldReturnNotFound_WhenLineItemMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new RemoveOrderLineItem.Command(order.Id, Guid.NewGuid()),
            ct);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(LineItemResult.Errors.NotFound(Guid.NewGuid()).Code);
    }

    private async Task<(Order Order, LineItem LineItem)> SeedOrderWithLineItem()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        var lineItem = LineItemMethod.Create(order.Id, Guid.NewGuid(), 2, 10m).Value;
        order.LineItems.Add(lineItem);

        _dbContext.Set<Order>().Add(order);
        _dbContext.Set<LineItem>().Add(lineItem);
        await _dbContext.SaveChangesAsync(ct);
        return (order, lineItem);
    }
}
