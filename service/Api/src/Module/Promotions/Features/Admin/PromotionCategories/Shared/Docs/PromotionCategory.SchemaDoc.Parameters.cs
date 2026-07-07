using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Docs;

/// <summary>OpenAPI schema documentation for PromotionCategoryParameters.</summary>
public class PromotionCategoryParametersDoc : SchemaDoc<PromotionCategoryParameters>
{
    public override void Configure(SchemaDocBuilder<PromotionCategoryParameters> builder)
    {
        builder.Property(x => x.Name)
            .HasDescription("The display name of the promotion category.")
            .HasExample("Seasonal");

        builder.Property(x => x.Code)
            .HasDescription("An optional unique code for the category.")
            .HasExample("SEASONAL");

        builder.Property(x => x.Presentation)
            .HasDescription("An optional display label for the category.")
            .HasExample("Seasonal Promotions");
    }
}
