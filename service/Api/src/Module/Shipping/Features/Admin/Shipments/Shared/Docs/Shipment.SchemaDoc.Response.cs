using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Docs;

public class ShipmentDetailResponseDoc : SchemaDoc<ShipmentDetailResponse>
{
    public override void Configure(SchemaDocBuilder<ShipmentDetailResponse> builder)
    {
        builder.Property(x => x.Id)
            .HasDescription("The unique identifier of the shipment.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.ShippedAtUtc)
            .HasDescription("The UTC timestamp when the shipment was shipped.");

        builder.Property(x => x.CreatedAtUtc)
            .HasDescription("The UTC timestamp when the shipment was created.");

        builder.Property(x => x.ModifiedAtUtc)
            .HasDescription("The UTC timestamp when the shipment was last modified.");
    }
}
