namespace Module.Ordering.Features.Admin.Orders.Shipments.Create;

public static partial class CreateOrderShipment
{
    public class Request
    {
        public Guid StockLocationId { get; init; }
        public Guid? ShippingMethodId { get; init; }
    }
}
