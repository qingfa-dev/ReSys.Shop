using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Persistence.Configurations.Payments;

// Configure: PaymentCapture EF Core mapping — table, keys, properties, relationships
public class PaymentConfiguration : IEntityTypeConfiguration<PaymentCapture>
{
    public void Configure(EntityTypeBuilder<PaymentCapture> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentCaptures, PaymentSchema.Name);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Number).IsRequired().HasMaxLength(PaymentConstant.Constraints.MaxPaymentNumberLength);
        builder.Property(x => x.Amount).HasPrecision(PaymentConstant.Constraints.Precision, PaymentConstant.Constraints.Scale);
        builder.Property(x => x.State).IsRequired().HasConversion<string>().HasDefaultValue(PaymentRecordState.Checkout);
        builder.Property(x => x.ResponseCode).HasMaxLength(PaymentConstant.Constraints.MaxResponseCodeLength);
        builder.Property(x => x.AvsResponse).HasMaxLength(PaymentConstant.Constraints.MaxAvsResponseLength);
        builder.Property(x => x.CvvResponseCode).HasMaxLength(PaymentConstant.Constraints.MaxCvvCodeLength);
        builder.Property(x => x.CvvResponseMessage).HasMaxLength(PaymentConstant.Constraints.MaxCvvMessageLength);
        builder.Property(x => x.IntentClientSecret).HasMaxLength(PaymentConstant.Constraints.MaxIntentClientSecretLength);
        builder.Property(x => x.CaptureEventCreated);
        builder.Property(x => x.RefundedAmount).HasPrecision(PaymentConstant.Constraints.Precision, PaymentConstant.Constraints.Scale);
        builder.Property(x => x.PaymentMethodId);
        builder.Property(x => x.OrderId);
        builder.Property(x => x.SourceId);
        builder.Property(x => x.SourceType).HasMaxLength(PaymentConstant.Constraints.MaxSourceTypeLength);
        builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(PaymentMethodConstant.Constraints.MaxProviderKeyLength);

        builder.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PaymentMethod).WithMany(pm => pm.Payments).HasForeignKey(x => x.PaymentMethodId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ResponseCode)
            .HasDatabaseName("ix_payment_captures_response_code")
            .HasFilter("\"ResponseCode\" IS NOT NULL");
    }
}