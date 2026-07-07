using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Storefront.Shared.Models;

namespace Module.Shipping.Features.Storefront.Shared.Docs;

public class ShippingMethodParametersDoc : SchemaDoc<ShippingMethodParameters>
{
    public override void Configure(SchemaDocBuilder<ShippingMethodParameters> builder)
    {
        builder.Property(x => x.MethodName)
            .HasDescription("The display name of the shipping method.")
            .HasExample("Standard Shipping");

        builder.Property(x => x.Description)
            .HasDescription("Description of the shipping method.")
            .HasExample("Delivered within 5-7 business days");

        builder.Property(x => x.Cost)
            .HasDescription("The cost of the shipping method.")
            .HasExample(5.99m);

        builder.Property(x => x.Currency)
            .HasDescription("The currency code.")
            .HasExample("USD");

        builder.Property(x => x.EstimatedDaysMin)
            .HasDescription("Minimum estimated delivery days.")
            .HasExample(3);

        builder.Property(x => x.EstimatedDaysMax)
            .HasDescription("Maximum estimated delivery days.")
            .HasExample(7);

        builder.Property(x => x.IsActive)
            .HasDescription("Whether the shipping method is active.")
            .HasExample(true);
    }
}
