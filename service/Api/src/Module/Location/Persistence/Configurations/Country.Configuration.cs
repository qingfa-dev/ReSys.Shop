using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Location.Domain.Countries;
using Module.Location.Persistence.Constants;

namespace Module.Location.Persistence.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // Table
        builder.ToTable(name: LocationSchema.TableNames.Countries, schema: LocationSchema.Name);

        // Key
        builder.HasKey(keyExpression: c => c.Id);

        // Properties
        builder.Property(propertyExpression: c => c.Name)
            .HasMaxLength(maxLength: CountryConstant.Constraints.MaxNameLength)
            .IsRequired();

        builder.Property(propertyExpression: c => c.IsoCode)
            .HasMaxLength(maxLength: CountryConstant.Constraints.MaxIsoCodeLength)
            .IsRequired();

        builder.Property(propertyExpression: c => c.CallingCode)
            .HasMaxLength(maxLength: CountryConstant.Constraints.MaxCallingCodeLength);

        builder.Property(propertyExpression: c => c.StatesRequired)
            .IsRequired()
            .HasDefaultValue(value: CountryConstant.Defaults.StatesRequired);

        builder.Property(propertyExpression: c => c.ZipcodeRequired)
            .IsRequired()
            .HasDefaultValue(value: false);

        builder.Property(propertyExpression: c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(value: CountryConstant.Defaults.IsActive);

        // Relationships
        builder.HasMany(navigationExpression: c => c.States)
            .WithOne(navigationExpression: s => s.Country)
            .HasForeignKey(foreignKeyExpression: s => s.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}