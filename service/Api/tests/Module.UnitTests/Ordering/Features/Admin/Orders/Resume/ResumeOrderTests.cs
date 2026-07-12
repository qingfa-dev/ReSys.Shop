using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Resume;

using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Resume;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "ResumeOrder")]
public class ResumeOrderTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<ResumeOrder.CommandHandler>> _loggerMock;
    private readonly ResumeOrder.CommandHandler _handler;

    public ResumeOrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<ResumeOrder.CommandHandler>>();
        _handler = new ResumeOrder.CommandHandler(_dbContext, _notificationServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should resume a canceled order")]
    public async Task Handle_ShouldResumeCanceledOrder()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Cancel first
        order.Cancel(Guid.NewGuid());
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ResumeOrder.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Placed);

        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { order.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);
        persisted.CanceledAtUtc.Should().BeNull();
        persisted.CanceledById.Should().BeNull();
    }

    [Fact(DisplayName = "Handler: Should return not found when order missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new ResumeOrder.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
