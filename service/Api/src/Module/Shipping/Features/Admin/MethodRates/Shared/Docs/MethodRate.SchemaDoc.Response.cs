using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.MethodRates.Shared.Models;

namespace Module.Shipping.Features.Admin.MethodRates.Shared.Docs;

public class MethodRateDetailResponseDoc : SchemaDoc<MethodRateDetailResponse>
{
    public override void Configure(SchemaDocBuilder<MethodRateDetailResponse> builder)
    {
        builder.Property(x => x.Id)
            .HasDescription("The unique identifier of the rate.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.CreatedAtUtc)
            .HasDescription("The UTC timestamp when the rate was created.");

        builder.Property(x => x.ModifiedAtUtc)
            .HasDescription("The UTC timestamp when the rate was last modified.");
    }
}
