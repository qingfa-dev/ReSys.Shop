using FluentAssertions;
using Module.Ordering.Domain.Orders.Events;
using Xunit;

namespace Module.UnitTests.Ordering.Domain.Orders.Events;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "DomainEvents")]
public sealed class OrderResumedEventTests
{
    [Fact(DisplayName = "OrderResumedEvent should initialize with correct data")]
    public void Constructor_ShouldInitializeWithCorrectData()
    {
        var orderId = Guid.NewGuid();
        var orderNumber = "R20260521-ABC123";
        var customerId = Guid.NewGuid();
        var email = "test@example.com";
        var resumedAt = DateTimeOffset.UtcNow;

        var e = new OrderResumedEvent(orderId, orderNumber, customerId, email, resumedAt);

        e.OrderId.Should().Be(orderId);
        e.OrderNumber.Should().Be(orderNumber);
        e.CustomerId.Should().Be(customerId);
        e.CustomerEmail.Should().Be(email);
        e.ResumedAtUtc.Should().Be(resumedAt);
        e.EntityId.Should().Be(orderId);
    }
}
