using Shared.Operational.Persistence.Configurations.Dictionaries;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Payment.Persistence.Constants;
using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Persistence.Configurations.PaymentMethods;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentMethods, PaymentSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PaymentMethodConstant.Constraints.MaxNameLength);

        builder.Property(x => x.Code)
            .HasMaxLength(PaymentMethodConstant.Constraints.MaxCodeLength);

        builder.Property(x => x.Description)
            .HasMaxLength(PaymentMethodConstant.Constraints.MaxDescriptionLength);

        builder.Property(x => x.ProviderType)
            .IsRequired()
            .HasMaxLength(PaymentMethodConstant.Constraints.MaxProviderTypeLength);

        builder.Property(x => x.Active)
            .IsRequired()
            .HasDefaultValue(PaymentMethodConstant.Defaults.Active);

        builder.Property(x => x.AutoCapture)
            .IsRequired()
            .HasDefaultValue(PaymentMethodConstant.Defaults.AutoCapture);

        builder.Property(x => x.DisplayOn)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PaymentMethodConstant.Defaults.DisplayOn);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(PaymentMethodConstant.Defaults.Position);

        builder.Property(x => x.Presentation);

        builder.Property(x => x.Preferences).HasConversion<DictionaryValueConverter<string, string>>();
        #endregion
    }
}
