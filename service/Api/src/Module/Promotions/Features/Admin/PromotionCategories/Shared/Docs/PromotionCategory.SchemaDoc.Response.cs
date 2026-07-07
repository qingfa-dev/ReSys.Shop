using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Docs;

/// <summary>OpenAPI schema documentation for PromotionCategoryDetailResponse.</summary>
public class PromotionCategoryDetailResponseDoc : SchemaDoc<PromotionCategoryDetailResponse>
{
    public override void Configure(SchemaDocBuilder<PromotionCategoryDetailResponse> builder)
    {
        builder.Property(x => x.Id)
            .HasDescription("The unique identifier of the promotion category.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.CreatedAtUtc)
            .HasDescription("The timestamp when the category was created.")
            .HasExample("2026-05-01T12:00:00Z");

        builder.Property(x => x.ModifiedAtUtc)
            .HasDescription("The timestamp when the category was last modified.")
            .HasExample("2026-05-15T12:00:00Z");
    }
}
