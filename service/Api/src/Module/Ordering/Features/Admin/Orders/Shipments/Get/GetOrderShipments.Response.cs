using Module.Shipping.Domain.Shipments;

namespace Module.Ordering.Features.Admin.Orders.Shipments.Get;
public static partial class GetOrderShipments
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public ShipmentState State { get; init; }
        public string? Tracking { get; init; }
        public decimal Cost { get; init; }
        public Guid? ShippingMethodId { get; init; }
        public Guid StockLocationId { get; init; }
        public DateTimeOffset? ShippedAtUtc { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
