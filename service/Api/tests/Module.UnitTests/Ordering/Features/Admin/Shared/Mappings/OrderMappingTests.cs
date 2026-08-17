using Microsoft.EntityFrameworkCore;

using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Mappings;
using Module.Ordering.Features.Admin.Shared.Models;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Admin.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderMapping")]
public class OrderMappingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public OrderMappingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact(DisplayName = "BuildTimeline: skips null timestamps and sorts ascending")]
    public void BuildTimeline_SkipsNulls_AndSortsAscending()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.CreatedAtUtc = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        order.CompletedAtUtc = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
        order.PaymentCompletedAtUtc = new DateTimeOffset(2026, 8, 1, 11, 0, 0, TimeSpan.Zero);
        order.ShipmentShippedAtUtc = new DateTimeOffset(2026, 8, 2, 9, 0, 0, TimeSpan.Zero);
        // PaymentFailedAtUtc / CanceledAtUtc / ApprovedAtUtc remain null.

        var timeline = OrderMapping.BuildTimeline(order);

        timeline.Select(e => e.Type).Should().Equal(OrderTimelineEventType.Created, OrderTimelineEventType.PaymentCompleted, OrderTimelineEventType.Placed, OrderTimelineEventType.Shipped);
        timeline.Should().BeInAscendingOrder(e => e.OccurredAtUtc);
    }

    [Fact(DisplayName = "MapToDetail: maps payments, shipments, and the five timestamps")]
    public async Task MapToDetail_MapsPaymentsShipmentsTimestamps()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.PaymentProcessingAtUtc = DateTimeOffset.UtcNow.AddMinutes(-30);
        order.PaymentCompletedAtUtc = DateTimeOffset.UtcNow;
        order.ShipmentShippedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5);
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        shipment.TrackingNumber = "TRK-1";
        shipment.Status = ShipmentStatus.Shipped;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        payment.State = PaymentRecordState.Completed;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        // Reload: the mapper reads the navigations off the tracked aggregate.
        var entity = await _dbContext.Set<Order>()
            .Include(x => x.PaymentCaptures)
            .Include(x => x.Shipments).ThenInclude(s => s.ShippingMethod)
            .AsNoTracking()
            .FirstAsync(x => x.Id == order.Id, ct);

        var response = entity.MapToDetail<OrderDetailResponse>();

        response.PaymentProcessingAtUtc.Should().Be(order.PaymentProcessingAtUtc);
        response.PaymentCompletedAtUtc.Should().Be(order.PaymentCompletedAtUtc);
        response.ShipmentShippedAtUtc.Should().Be(order.ShipmentShippedAtUtc);
        response.PaymentFailedAtUtc.Should().BeNull();
        response.Payments.Should().ContainSingle().Which.Number.Should().Be(payment.Number);
        response.Shipments.Should().ContainSingle().Which.TrackingNumber.Should().Be("TRK-1");
        response.Timeline.Should().NotBeEmpty();
    }

    public void Dispose() => _dbContext.Dispose();
}
