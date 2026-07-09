using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Payment.Domain.Payments;
using Module.Payment.Persistence.Constants;

namespace Module.Payment.Persistence.Configurations.Payments;

public class PaymentConfiguration : IEntityTypeConfiguration<global::Module.Payment.Domain.Payments.PaymentRecord>
{
    public void Configure(EntityTypeBuilder<global::Module.Payment.Domain.Payments.PaymentRecord> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentRecords, PaymentSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(PaymentConstant.Constraints.MaxNumberLength);

        builder.Property(x => x.Amount)
            .HasPrecision(PaymentConstant.Constraints.Precision, PaymentConstant.Constraints.Scale);

        builder.Property(x => x.State)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PaymentRecordState.Checkout);

        builder.Property(x => x.ResponseCode)
            .HasMaxLength(PaymentConstant.Constraints.MaxResponseCodeLength);

        builder.Property(x => x.AvsResponse)
            .HasMaxLength(PaymentConstant.Constraints.MaxAvsResponseLength);

        builder.Property(x => x.CvvResponseCode)
            .HasMaxLength(PaymentConstant.Constraints.MaxCvvCodeLength);

        builder.Property(x => x.CvvResponseMessage)
            .HasMaxLength(PaymentConstant.Constraints.MaxCvvMessageLength);

        builder.Property(x => x.IntentClientSecret)
            .HasMaxLength(PaymentConstant.Constraints.MaxIntentClientSecretLength);

        builder.Property(x => x.PaymentMethodId);
        builder.Property(x => x.OrderId);
        builder.Property(x => x.SourceId);
        builder.Property(x => x.SourceType)
            .HasMaxLength(PaymentConstant.Constraints.MaxSourceTypeLength);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany(pm => pm.Payments)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);
        #endregion
    }
}
