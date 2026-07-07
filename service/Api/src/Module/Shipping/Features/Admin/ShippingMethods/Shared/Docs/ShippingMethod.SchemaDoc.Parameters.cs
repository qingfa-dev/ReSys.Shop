using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.Shipping.Features.Admin.ShippingMethods.Shared.Docs;

public class ShippingMethodParametersDoc : SchemaDoc<ShippingMethodParameters>
{
    public override void Configure(SchemaDocBuilder<ShippingMethodParameters> builder)
    {
        builder.Property(x => x.Name)
            .HasDescription("The name of the shipping method.")
            .HasExample("Standard Shipping");

        builder.Property(x => x.CalculatorType)
            .HasDescription("The calculator type used for rate computation.")
            .HasExample("FlatRate");

        builder.Property(x => x.Code)
            .HasDescription("Optional unique code for the shipping method.")
            .HasExample("STANDARD");

        builder.Property(x => x.TaxCategoryId)
            .HasDescription("The tax category identifier.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.TrackingUrl)
            .HasDescription("URL template for tracking shipments; use {tracking} placeholder.");

        builder.Property(x => x.AdminName)
            .HasDescription("Internal admin-only display name.");

        builder.Property(x => x.Position)
            .HasDescription("Display order position (ascending).")
            .HasExample(0);

        builder.Property(x => x.AvailableToUsers)
            .HasDescription("Whether this method is available to storefront customers.")
            .HasExample(true);
    }
}
