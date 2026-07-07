using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Docs;

public class ShippingMethodDetailResponseDoc : SchemaDoc<ShippingMethodDetailResponse>
{
    public override void Configure(SchemaDocBuilder<ShippingMethodDetailResponse> builder)
    {
        builder.Property(x => x.Id)
            .HasDescription("The unique identifier of the shipping method.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.CreatedAtUtc)
            .HasDescription("The UTC timestamp when the shipping method was created.");

        builder.Property(x => x.ModifiedAtUtc)
            .HasDescription("The UTC timestamp when the shipping method was last modified.");
    }
}
