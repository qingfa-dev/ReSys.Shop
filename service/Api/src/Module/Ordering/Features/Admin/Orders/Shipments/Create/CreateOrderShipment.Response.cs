namespace Module.Ordering.Features.Admin.Orders.Shipments.Create;

public static partial class CreateOrderShipment
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public Guid OrderId { get; init; }
        public Guid StockLocationId { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
