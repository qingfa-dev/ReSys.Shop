using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Approve;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Approve;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "ApproveOrder")]
public class ApproveOrderTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly ApproveOrder.CommandHandler _handler;

    public ApproveOrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _handler = new ApproveOrder.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should approve a placed order")]
    public async Task Handle_ShouldApproveOrder_WhenPlaced()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ApproveOrder.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ApprovedById.Should().NotBeNull();
        result.Value.ApprovedAtUtc.Should().NotBeNull();

        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { order.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.ApprovedById.Should().NotBeNull();
        persisted.ApprovedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handler: Should return not found when order missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new ApproveOrder.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
