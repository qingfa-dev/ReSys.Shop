using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionRules.Shared.Docs;

public class PromotionRuleDetailResponseDoc : SchemaDoc<PromotionRuleDetailResponse>
{
    public override void Configure(SchemaDocBuilder<PromotionRuleDetailResponse> builder)
    {
        builder.Property(x => x.Id)
            .HasDescription("The unique identifier of the promotion rule.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.PromotionId)
            .HasDescription("The unique identifier of the parent promotion.")
            .HasExample("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        builder.Property(x => x.CreatedAtUtc)
            .HasDescription("The timestamp when the rule was created.")
            .HasExample("2026-05-01T12:00:00Z");
    }
}
