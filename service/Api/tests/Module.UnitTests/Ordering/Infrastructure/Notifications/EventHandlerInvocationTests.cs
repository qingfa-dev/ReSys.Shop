using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Cancel;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.UnitTests.Ordering.Infrastructure.Notifications;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "Notifications")]
public sealed class EventHandlerInvocationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CancelOrderAdmin.CommandHandler>> _loggerMock;

    public EventHandlerInvocationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.UserName).Returns("test-user");

        _loggerMock = new Mock<ILogger<CancelOrderAdmin.CommandHandler>>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "CancelOrderAdmin handler should send OrderCancelled notification")]
    public async Task CancelOrderAdmin_ShouldSendOrderCancelledNotification()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Email = "test@example.com";
        order.Number = "R20260521-TEST01";
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CancelOrderAdmin.CommandHandler(
            _dbContext,
            _currentUserMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        var result = await handler.Handle(
            new CancelOrderAdmin.Command(order.Id, new CancelOrderAdmin.Request { Reason = "test" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _notificationServiceMock.Verify(
            x => x.SendAsync(
                It.Is<NotificationMessage>(m =>
                    m.UseCase == NotificationUseCase.OrderCancelled &&
                    m.Recipient.Identifier == "test@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
