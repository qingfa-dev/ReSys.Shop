using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Docs;

public class ShippingMethodUpdateParametersDoc : SchemaDoc<ShippingMethodUpdateParameters>
{
    public override void Configure(SchemaDocBuilder<ShippingMethodUpdateParameters> builder)
    {
        builder.Property(x => x.Name)
            .HasDescription("The name of the shipping method.")
            .HasExample("Express Shipping");

        builder.Property(x => x.CalculatorType)
            .HasDescription("The calculator type used for rate computation.")
            .HasExample("WeightBased");

        builder.Property(x => x.Code)
            .HasDescription("Optional unique code for the shipping method.")
            .HasExample("EXPRESS");

        builder.Property(x => x.AvailableToUsers)
            .HasDescription("Whether this method is available to storefront customers.")
            .HasExample(true);

        builder.Property(x => x.Presentation)
            .HasDescription("The display presentation text.");
    }
}
