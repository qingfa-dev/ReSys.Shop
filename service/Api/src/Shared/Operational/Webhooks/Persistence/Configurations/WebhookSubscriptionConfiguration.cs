using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Operational.Webhooks.Domain;

namespace Shared.Operational.Webhooks.Persistence.Configurations;

public sealed class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable(WebhookSchema.TableNames.Subscriptions, WebhookSchema.Name);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Event).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.SecretHash).IsRequired().HasMaxLength(256);
        builder.Property(x => x.HeadersJson).HasColumnType("jsonb");
        builder.HasIndex(x => x.Event);
        builder.HasIndex(x => new { x.Event, x.Active });
    }
}
