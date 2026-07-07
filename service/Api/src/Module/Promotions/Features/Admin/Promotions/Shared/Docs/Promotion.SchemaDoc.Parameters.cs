using BuildingBlocks.OpenApi.Metadata.Schemas;
using Module.Promotions.Features.Admin.Promotions.Shared.Models;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Docs;

/// <summary>OpenAPI schema documentation for PromotionParameters.</summary>
public class PromotionParametersDoc : SchemaDoc<PromotionParameters>
{
    public override void Configure(SchemaDocBuilder<PromotionParameters> builder)
    {
        builder.Property(x => x.Name)
            .HasDescription("The display name of the promotion.")
            .HasExample("Summer Sale");

        builder.Property(x => x.Code)
            .HasDescription("An optional unique code for automatic promotions.")
            .HasExample("SUMMER20");

        builder.Property(x => x.Description)
            .HasDescription("A description of the promotion.")
            .HasExample("20% off summer collection");

        builder.Property(x => x.UsageLimit)
            .HasDescription("Maximum number of times this promotion can be used.")
            .HasExample(100);

        builder.Property(x => x.PerCustomerUsageLimit)
            .HasDescription("Maximum uses per customer.")
            .HasExample(1);

        builder.Property(x => x.StartsAtUtc)
            .HasDescription("The start date for promotion eligibility.")
            .HasExample("2026-06-01T00:00:00Z");

        builder.Property(x => x.ExpiresAtUtc)
            .HasDescription("The end date for promotion eligibility.")
            .HasExample("2026-06-30T23:59:59Z");

        builder.Property(x => x.MatchPolicy)
            .HasDescription("How promotion rules are evaluated (All/Any).")
            .HasExample("All");

        builder.Property(x => x.Kind)
            .HasDescription("Whether this is a coupon-code or automatic promotion.")
            .HasExample("Automatic");

        builder.Property(x => x.Advertise)
            .HasDescription("Whether to display the promotion to customers.")
            .HasExample(false);

        builder.Property(x => x.Active)
            .HasDescription("Whether the promotion is currently active.")
            .HasExample(true);

        builder.Property(x => x.Position)
            .HasDescription("Display ordering position (0-based ascending).")
            .HasExample(0);

        builder.Property(x => x.Path)
            .HasDescription("Optional URL path for the promotion landing page.")
            .HasExample("/promotions/summer-sale");
    }
}
