using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Persistence.Constants;

public static class ShippingSchema
{
    public const string Name = "shipping";

    public static class TableNames
    {
        public static string Shipments => nameof(Shipment).ToSnakeCase()!;
        public static string ShippingMethods => nameof(ShippingMethod).ToSnakeCase()!;
        public static string ShippingRates => nameof(ShippingRate).ToSnakeCase()!;
    }
}
