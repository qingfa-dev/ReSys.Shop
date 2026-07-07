using FluentAssertions;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Events;
using Module.Ordering.Features.Admin.Orders.ResendConfirmationEmail;
using Module.Ordering.Features.Admin.Orders.Resume;
using Xunit;

namespace Module.UnitTests.Ordering.Infrastructure.Notifications;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "Notifications")]
public sealed class EventHandlerInvocationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public EventHandlerInvocationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "ResumeOrder handler should raise OrderResumedEvent on tracked entity")]
    public async Task ResumeOrder_ShouldRaiseOrderResumedEvent()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Email = "test@example.com";
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        order.Cancel(Guid.NewGuid());
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ResumeOrder.CommandHandler(_dbContext);
        var result = await handler.Handle(
            new ResumeOrder.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderResumedEvent>();
        var evt = (OrderResumedEvent)order.DomainEvents.Single();
        evt.OrderId.Should().Be(order.Id);
        evt.OrderNumber.Should().Be(order.Number);
        evt.CustomerEmail.Should().Be("test@example.com");
    }

    [Fact(DisplayName = "ResendOrderConfirmationEmail handler should raise OrderPlacedEvent")]
    public async Task ResendConfirmation_ShouldRaiseOrderPlacedEvent()
    {
        var order = OrderExtensions.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Email = "test@example.com";
        order.Number = "R20260521-TEST01";
        order.Total = 59.99m;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new ResendOrderConfirmationEmail.CommandHandler(_dbContext);
        var result = await handler.Handle(
            new ResendOrderConfirmationEmail.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderPlacedEvent>();
        var evt = (OrderPlacedEvent)order.DomainEvents.Single();
        evt.OrderId.Should().Be(order.Id);
        evt.OrderNumber.Should().Be(order.Number);
        evt.CustomerEmail.Should().Be("test@example.com");
        evt.Total.Should().Be(59.99m);
    }
}
