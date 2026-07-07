using FluentAssertions;
using Module.Ordering.Domain.Orders.Events;
using Xunit;

namespace Module.UnitTests.Ordering.Domain.Orders.Events;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "DomainEvents")]
public sealed class OrderCanceledEventTests
{
    [Fact(DisplayName = "OrderCanceledEvent should initialize with correct data")]
    public void Constructor_ShouldInitializeWithCorrectData()
    {
        var orderId = Guid.NewGuid();
        var orderNumber = "R20260521-ABC123";
        var customerId = Guid.NewGuid();
        var email = "test@example.com";
        var canceledAt = DateTimeOffset.UtcNow;
        var canceledBy = "admin@example.com";

        var e = new OrderCanceledEvent(orderId, orderNumber, customerId, email, canceledAt, canceledBy);

        e.OrderId.Should().Be(orderId);
        e.OrderNumber.Should().Be(orderNumber);
        e.CustomerId.Should().Be(customerId);
        e.CustomerEmail.Should().Be(email);
        e.CanceledAtUtc.Should().Be(canceledAt);
        e.CanceledBy.Should().Be(canceledBy);
        e.EntityId.Should().Be(orderId);
    }

    [Fact(DisplayName = "OrderCanceledEvent should allow null CanceledBy")]
    public void Constructor_ShouldAllowNullCanceledBy()
    {
        var e = new OrderCanceledEvent(Guid.NewGuid(), "NUM", Guid.NewGuid(), "email@test.com", DateTimeOffset.UtcNow, null);

        e.CanceledBy.Should().BeNull();
    }
}
