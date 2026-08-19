using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Billing.Domain.PaymentMethods;

using Shared.Operational.Persistence.Configurations.Dictionaries;

namespace Module.Billing.Persistence.Configurations.PaymentMethods;

// Configure: PaymentMethod EF Core mapping — table, keys, properties, converters
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable(PaymentSchema.TableNames.PaymentMethods, PaymentSchema.Name);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(PaymentMethodConstant.Constraints.MaxNameLength);
        builder.Property(x => x.Code).HasMaxLength(PaymentMethodConstant.Constraints.MaxCodeLength);
        builder.Property(x => x.Description).HasMaxLength(PaymentMethodConstant.Constraints.MaxDescriptionLength);
        builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(PaymentMethodConstant.Constraints.MaxProviderKeyLength);
        builder.Property(x => x.Active).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.AutoCapture).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DisplayOn).IsRequired().HasConversion<string>().HasDefaultValue(DisplayOn.Both);
        builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.Presentation)
            .HasMaxLength(PaymentMethodConstant.Constraints.MaxPresentationLength);
        builder.Property(x => x.WebhookEnabled).HasDefaultValue(false);

        builder.Property(x => x.Preferences)
            .HasConversion<DictionaryValueConverter<string, string>>()
            .HasColumnType("jsonb");

        builder.Property(x => x.Settings)
            .HasConversion<EncryptedDictionaryConverter>()
            .HasColumnType("text");
    }
}