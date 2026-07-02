using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Profile.Domain.Addresses;
using Module.Profile.Persistence.Constants;

namespace Module.Profile.Persistence.Configurations;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable(ProfileSchema.TableNames.Addresses, ProfileSchema.Name);

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AddressType)
            .IsRequired();

        builder.Property(a => a.FirstName)
            .HasMaxLength(AddressConstant.Constraints.MaxFirstNameLength)
            .IsRequired();

        builder.Property(a => a.LastName)
            .HasMaxLength(AddressConstant.Constraints.MaxLastNameLength);

        builder.Property(a => a.Address1)
            .HasMaxLength(AddressConstant.Constraints.MaxAddress1Length)
            .IsRequired();

        builder.Property(a => a.Address2)
            .HasMaxLength(AddressConstant.Constraints.MaxAddress2Length);

        builder.Property(a => a.City)
            .HasMaxLength(AddressConstant.Constraints.MaxCityLength)
            .IsRequired();

        builder.Property(a => a.ZipCode)
            .HasMaxLength(AddressConstant.Constraints.MaxZipCodeLength);

        builder.Property(a => a.Phone)
            .HasMaxLength(AddressConstant.Constraints.MaxPhoneLength);

        builder.Property(a => a.Label)
            .HasMaxLength(AddressConstant.Constraints.MaxLabelLength);

        builder.Property(a => a.IsDefault)
            .IsRequired()
            .HasDefaultValue(AddressConstant.Defaults.IsDefault);

        builder.Property(a => a.IsDefaultBilling)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.IsDefaultShipping)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.CountryName)
            .HasMaxLength(AddressConstant.Constraints.MaxCountryNameLength)
            .IsRequired();

        builder.Property(a => a.StateProvince)
            .HasMaxLength(AddressConstant.Constraints.MaxStateProvinceLength);

        builder.Property(a => a.CountryCode)
            .HasMaxLength(AddressConstant.Constraints.MaxCountryCodeLength);

        builder.Property(a => a.StateCode)
            .HasMaxLength(AddressConstant.Constraints.MaxStateCodeLength);

        builder.HasOne(p => p.UserProfile)
            .WithMany(p => p.Addresses)
            .HasForeignKey(p => p.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserProfileId);

        builder.HasIndex(p => new { p.AddressType, p.UserProfileId });

        builder.HasIndex(p => p.CountryCode);
    }
}