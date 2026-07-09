using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Operational.Webhooks.Domain;

namespace Shared.Operational.Webhooks.Persistence.Configurations;

public sealed class WebhookSubscriptionEntityConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        // Table
        builder.ToTable(name: WebhookSchema.TableNames.Subscriptions, schema: WebhookSchema.Name);

        // Key
        builder.HasKey(keyExpression: x => x.Id);

        // Properties
        builder.Property(propertyExpression: x => x.Event)
            .IsRequired()
            .HasMaxLength(maxLength: WebhookSubscriptionConstant.Constraints.Event.MaxLength);

        builder.Property(propertyExpression: x => x.Url)
            .IsRequired()
            .HasMaxLength(maxLength: WebhookSubscriptionConstant.Constraints.Url.MaxLength);

        builder.Property(propertyExpression: x => x.SecretHash)
            .IsRequired()
            .HasMaxLength(maxLength: WebhookSubscriptionConstant.Constraints.SecretHash.MaxLength);

        builder.Property(propertyExpression: x => x.HeadersJson)
            .HasColumnType(typeName: WebhookSubscriptionConstant.Constraints.HeadersJson.ColumnType);

        // Indexes
        builder.HasIndex(x => x.Event);
        builder.HasIndex(x => new { x.Event, x.Active });
    }
}