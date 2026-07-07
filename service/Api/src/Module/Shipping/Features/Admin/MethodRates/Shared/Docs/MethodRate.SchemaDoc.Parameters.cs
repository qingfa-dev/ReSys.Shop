using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.MethodRates.Shared.Models;

namespace Module.Shipping.Features.Admin.MethodRates.Shared.Docs;

public class MethodRateParametersDoc : SchemaDoc<MethodRateParameters>
{
    public override void Configure(SchemaDocBuilder<MethodRateParameters> builder)
    {
        builder.Property(x => x.Name)
            .HasDescription("The name of the shipping rate.")
            .HasExample("Standard");

        builder.Property(x => x.Cost)
            .HasDescription("The base cost of the rate.")
            .HasExample(5.99m);

        builder.Property(x => x.FinalPrice)
            .HasDescription("The final price after adjustments.")
            .HasExample(5.99m);

        builder.Property(x => x.DeliveryRange)
            .HasDescription("Estimated delivery range (e.g., '3-5 business days').")
            .HasExample("3-5 business days");

        builder.Property(x => x.ShippingMethodId)
            .HasDescription("The shipping method identifier.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    }
}
