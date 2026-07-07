using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.Promotions.Shared.Models;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Docs;

/// <summary>OpenAPI schema documentation for PromotionDetailResponse.</summary>
public class PromotionDetailResponseDoc : SchemaDoc<PromotionDetailResponse>
{
    public override void Configure(SchemaDocBuilder<PromotionDetailResponse> builder)
    {
        builder.Property(x => x.Id)
            .HasDescription("The unique identifier of the promotion.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.CreatedAtUtc)
            .HasDescription("The timestamp when the promotion was created.")
            .HasExample("2026-05-01T12:00:00Z");

        builder.Property(x => x.ModifiedAtUtc)
            .HasDescription("The timestamp when the promotion was last modified.")
            .HasExample("2026-05-15T12:00:00Z");

        builder.Property(x => x.DeletedAtUtc)
            .HasDescription("The timestamp when the promotion was soft-deleted.");

        builder.Property(x => x.IsDeleted)
            .HasDescription("Whether the promotion has been soft-deleted.")
            .HasExample(false);
    }
}
