namespace Module.Shipping.Domain.Shipments;

public static class ShipmentConstant
{
    // Validate: Enforce domain constraints for shipment fields
    public static class Constraints
    {
        public const int MaxTrackingLength = 200;
        public const int Precision = 18;
        public const int Scale = 2;
    }

    // Initialize: Default values for newly created shipments
    public static class Defaults
    {
        public const ShipmentStatus Status = ShipmentStatus.Pending;
    }

    // Filter: Allowed search, sort, and filter fields for query operations
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Shipment.TrackingNumber),
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Shipment.TrackingNumber),
            nameof(Shipment.Status),
            nameof(Shipment.ShippedAtUtc),
            nameof(Shipment.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Shipment.Status),
            nameof(Shipment.OrderId),
            nameof(Shipment.ShippingMethodId)
        ];
    }
}