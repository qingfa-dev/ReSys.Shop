using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Operational.Webhooks.Domain;

namespace Shared.Operational.Webhooks.Persistence.Configurations;

public sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable(WebhookSchema.TableNames.Deliveries, WebhookSchema.Name);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Event).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PayloadJson).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.LastError).HasMaxLength(2048);
        builder.HasOne<WebhookSubscription>()
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Status, x.NextRetryAtUtc });
    }
}
