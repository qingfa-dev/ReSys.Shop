using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.PromotionActions.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionActions.Shared.Docs;

public class PromotionActionParametersDoc : SchemaDoc<PromotionActionParameters>
{
    public override void Configure(SchemaDocBuilder<PromotionActionParameters> builder)
    {
        builder.Property(x => x.Type)
            .HasDescription("The type of action (e.g., CreateAdjustment, FreeShipping).")
            .HasExample("CreateAdjustment");

        builder.Property(x => x.Preferences)
            .HasDescription("Key-value preferences dictating action behavior (e.g., amount, percent, label).")
            .HasExample("{\"amount\": \"10\", \"label\": \"Discount\"}");

        builder.Property(x => x.CalculatorType)
            .HasDescription("The calculator used for computing adjustment amounts (e.g., FlatRate, Percent).")
            .HasExample("FlatRate");
    }
}
