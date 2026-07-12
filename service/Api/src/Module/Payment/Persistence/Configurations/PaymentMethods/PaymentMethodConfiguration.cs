using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Module.Payment.Domain.PaymentMethods;

using Shared.Operational.Persistence.Configurations.Dictionaries;

namespace Module.Payment.Persistence.Configurations.PaymentMethods;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentMethods, PaymentSchema.Name);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(PaymentMethodConstant.Constraints.MaxProviderKeyLength);
        builder.Property(x => x.Active).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.AutoCapture).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DisplayOn).IsRequired().HasConversion<string>().HasDefaultValue(DisplayOn.Both);
        builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.Presentation);
        builder.Property(x => x.WebhookEnabled).HasDefaultValue(false);

        builder.Property(x => x.Preferences)
            .HasConversion<DictionaryValueConverter<string, string>>()
            .HasColumnType("jsonb");

        builder.Property(x => x.Settings)
            .HasConversion<EncryptedDictionaryConverter>()
            .HasColumnType("text");
    }
}
