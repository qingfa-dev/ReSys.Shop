using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Billing.Persistence;
using Module.Billing.Domain.WebhookEvents;

namespace Module.Billing.Persistence.Configurations.Webhooks;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.WebhookEvents, PaymentSchema.Name);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StripeEventId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Type).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.State)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(WebhookEventState.Pending);
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => x.StripeEventId).IsUnique();
    }
}
