using FluentAssertions;
using Module.Ordering.Domain.Orders.Events;
using Xunit;

namespace Module.UnitTests.Ordering.Domain.Orders.Events;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "DomainEvents")]
public sealed class OrderPlacedEventTests
{
    [Fact(DisplayName = "OrderPlacedEvent should initialize with correct data")]
    public void Constructor_ShouldInitializeWithCorrectData()
    {
        var orderId = Guid.NewGuid();
        var orderNumber = "R20260521-ABC123";
        var customerId = Guid.NewGuid();
        var email = "test@example.com";
        var total = 99.99m;
        var placedAt = DateTimeOffset.UtcNow;

        var e = new OrderPlacedEvent(orderId, orderNumber, customerId, email, total, placedAt);

        e.OrderId.Should().Be(orderId);
        e.OrderNumber.Should().Be(orderNumber);
        e.CustomerId.Should().Be(customerId);
        e.CustomerEmail.Should().Be(email);
        e.Total.Should().Be(total);
        e.PlacedAtUtc.Should().Be(placedAt);
        e.EntityId.Should().Be(orderId);
        e.EventId.Should().NotBeNullOrWhiteSpace();
        e.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
