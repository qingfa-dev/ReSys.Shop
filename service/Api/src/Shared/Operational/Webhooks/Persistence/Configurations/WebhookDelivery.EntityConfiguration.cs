using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Operational.Webhooks.Domain;

namespace Shared.Operational.Webhooks.Persistence.Configurations;

public sealed class WebhookDeliveryEntityConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        // Table
        builder.ToTable(name: WebhookSchema.TableNames.Deliveries, schema: WebhookSchema.Name);

        // Key
        builder.HasKey(keyExpression: x => x.Id);

        // Properties
        builder.Property(propertyExpression: x => x.Event)
            .IsRequired()
            .HasMaxLength(maxLength: WebhookDeliveryConstant.Constraints.Event.MaxLength);

        builder.Property(propertyExpression: x => x.PayloadJson)
            .IsRequired()
            .HasColumnType(typeName: WebhookDeliveryConstant.Constraints.PayloadJson.ColumnType);

        builder.Property(propertyExpression: x => x.LastError)
            .HasMaxLength(maxLength: WebhookDeliveryConstant.Constraints.LastError.MaxLength);

        // Relationships
        builder.HasOne<WebhookSubscription>()
            .WithMany()
            .HasForeignKey(foreignKeyExpression: x => x.SubscriptionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.Status, x.NextRetryAtUtc });
    }
}