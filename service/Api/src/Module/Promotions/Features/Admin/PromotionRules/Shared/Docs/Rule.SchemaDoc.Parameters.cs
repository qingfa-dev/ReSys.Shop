using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionRules.Shared.Docs;

public class PromotionRuleParametersDoc : SchemaDoc<PromotionRuleParameters>
{
    public override void Configure(SchemaDocBuilder<PromotionRuleParameters> builder)
    {
        builder.Property(x => x.Type)
            .HasDescription("The type of rule (e.g., ItemTotal, Product, UserRole, User).")
            .HasExample("ItemTotal");

        builder.Property(x => x.Preferences)
            .HasDescription("Key-value preferences dictating rule behavior (e.g., amount_min, products, roles).")
            .HasExample("{\"amount_min\": \"100\"}");
    }
}
