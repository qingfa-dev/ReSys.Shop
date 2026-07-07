using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Payment.Persistence.Constants;
using Module.Payment.Domain.PaymentCaptureEvents;

namespace Module.Payment.Persistence.Configurations.Payments;

public class PaymentCaptureEventConfiguration : IEntityTypeConfiguration<PaymentCaptureEvent>
{
    public void Configure(EntityTypeBuilder<PaymentCaptureEvent> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentCaptureEvents, PaymentSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Amount)
            .HasPrecision(PaymentCaptureEventConstant.Constraints.Precision, PaymentCaptureEventConstant.Constraints.Scale);

        builder.Property(x => x.PaymentId);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Payment)
            .WithMany(p => p.CaptureEvents)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}
