namespace Module.Shipping.Domain.Shipments;

public enum ShipmentState
{
    Pending,
    Ready,
    Shipped,
    Canceled
}

public static class ShipmentConstant
{
    // Validate: Enforce domain constraints for shipment fields
    public static class Constraints
    {
        public const int MaxNumberLength = 50;
        public const int MaxTrackingLength = 255;
        public const int Precision = 18;
        public const int Scale = 2;
    }

    // Initialize: Default values for newly created shipments
    public static class Defaults
    {
        public const ShipmentState State = ShipmentState.Pending;
    }

    // Filter: Allowed search, sort, and filter fields for query operations
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Shipment.Number),
            nameof(Shipment.Tracking)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Shipment.Number),
            nameof(Shipment.State),
            nameof(Shipment.Cost),
            nameof(Shipment.ShippedAtUtc),
            nameof(Shipment.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Shipment.State),
            nameof(Shipment.OrderId),
            nameof(Shipment.StockLocationId),
            nameof(Shipment.ShippingMethodId)
        ];
    }
}