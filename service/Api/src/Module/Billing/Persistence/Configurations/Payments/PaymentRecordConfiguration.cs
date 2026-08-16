using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Domain.PaymentMethods;

namespace Module.Billing.Persistence.Configurations.Payments;

// Configure: PaymentCapture EF Core mapping — table, keys, properties, relationships
public class PaymentConfiguration : IEntityTypeConfiguration<PaymentCapture>
{
    public void Configure(EntityTypeBuilder<PaymentCapture> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentCaptures, PaymentSchema.Name);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Number).IsRequired().HasMaxLength(PaymentConstant.Constraints.MaxPaymentNumberLength);
        builder.Property(x => x.Amount).HasPrecision(PaymentConstant.Constraints.Precision, PaymentConstant.Constraints.Scale);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(PaymentConstant.Constraints.MaxCurrencyLength).HasDefaultValue(PaymentConstant.Defaults.Currency);
        builder.Property(x => x.State).IsRequired().HasConversion<string>().HasDefaultValue(PaymentRecordState.Checkout);
        builder.Property(x => x.ResponseCode).HasMaxLength(PaymentConstant.Constraints.MaxResponseCodeLength);
        builder.Property(x => x.StripeSessionId).HasMaxLength(PaymentConstant.Constraints.MaxStripeSessionIdLength);
        builder.Property(x => x.StripePaymentIntentId).HasMaxLength(PaymentConstant.Constraints.MaxStripePaymentIntentIdLength);
        builder.Property(x => x.ProcessedAtUtc);
        builder.Property(x => x.AvsResponse).HasMaxLength(PaymentConstant.Constraints.MaxAvsResponseLength);
        builder.Property(x => x.CvvResponseCode).HasMaxLength(PaymentConstant.Constraints.MaxCvvCodeLength);
        builder.Property(x => x.CvvResponseMessage).HasMaxLength(PaymentConstant.Constraints.MaxCvvMessageLength);
        builder.Property(x => x.IntentClientSecret).HasMaxLength(PaymentConstant.Constraints.MaxIntentClientSecretLength);
        builder.Property(x => x.CheckoutUrl).HasMaxLength(PaymentConstant.Constraints.MaxCheckoutUrlLength);
        builder.Property(x => x.RefundedAmount).HasPrecision(PaymentConstant.Constraints.Precision, PaymentConstant.Constraints.Scale);
        builder.Property(x => x.CapturedAmount).HasPrecision(PaymentConstant.Constraints.Precision, PaymentConstant.Constraints.Scale);
        builder.Property(x => x.PaymentMethodId);
        builder.Property(x => x.OrderId);
        builder.Property(x => x.SourceId).HasMaxLength(PaymentConstant.Constraints.MaxSourceIdLength);
        builder.Property(x => x.SourceType).HasMaxLength(PaymentConstant.Constraints.MaxSourceTypeLength);
        builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(PaymentMethodConstant.Constraints.MaxProviderKeyLength);
        builder.Property(x => x.ProcessedStripeEventIds).HasColumnType("jsonb");
        builder.Property(x => x.LastStripeEventId).HasMaxLength(PaymentConstant.Constraints.MaxLastStripeEventIdLength);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Order)
            .WithMany(o => o.PaymentCaptures)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PaymentMethod)
            .WithMany(pm => pm.Payments)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.State)
            .HasDatabaseName("ix_payment_captures_state");
    }
}