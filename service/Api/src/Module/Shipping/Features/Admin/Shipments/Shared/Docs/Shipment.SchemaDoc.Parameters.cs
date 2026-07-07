using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Docs;

public class ShipmentParametersDoc : SchemaDoc<ShipmentParameters>
{
    public override void Configure(SchemaDocBuilder<ShipmentParameters> builder)
    {
        builder.Property(x => x.Number)
            .HasDescription("The shipment tracking number.");

        builder.Property(x => x.Tracking)
            .HasDescription("External tracking identifier.");

        builder.Property(x => x.Cost)
            .HasDescription("The base shipping cost.")
            .HasExample(9.99m);

        builder.Property(x => x.FinalPrice)
            .HasDescription("The final price after discounts and taxes.")
            .HasExample(9.99m);

        builder.Property(x => x.OrderId)
            .HasDescription("The associated order identifier.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.StockLocationId)
            .HasDescription("The stock location from which the shipment originates.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.ShippingMethodId)
            .HasDescription("The shipping method used for this shipment.");

        builder.Property(x => x.AddressId)
            .HasDescription("The shipping address identifier.");
    }
}
