using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Customer.Domain.Preferences;

namespace Module.Customer.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable(ProfileSchema.TableNames.Profiles, ProfileSchema.Name);

        builder.HasKey(p => p.UserId);

        builder.Property(p => p.UserId)
            .ValueGeneratedNever();

        builder.Property(p => p.FirstName)
            .HasMaxLength(UserProfileConstant.Constraints.MaxFirstNameLength)
            .IsRequired();

        builder.Property(p => p.LastName)
            .HasMaxLength(UserProfileConstant.Constraints.MaxLastNameLength)
            .IsRequired();

        builder.Property(p => p.Email)
            .HasMaxLength(UserProfileConstant.Constraints.MaxEmailLength)
            .IsRequired();

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(UserProfileConstant.Constraints.MaxPhoneNumberLength);

        builder.Property(p => p.DateOfBirth);

        builder.Property(p => p.Gender)
            .HasMaxLength(UserProfileConstant.Constraints.MaxGenderLength);

        builder.Property(p => p.Bio)
            .HasMaxLength(UserProfileConstant.Constraints.MaxBioLength);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(UserProfileConstant.Constraints.MaxAvatarUrlLength);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(UserProfileConstant.Defaults.IsActive);

        builder.Property(p => p.AcceptsEmailMarketing)
            .HasDefaultValue(false);

        builder.Property(p => p.OrdersCount)
            .HasDefaultValue(0);

        builder.Property(p => p.TotalSpent)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.InternalNoteHtml)
            .HasMaxLength(UserProfileConstant.Constraints.MaxInternalNoteLength);

        builder.Property(p => p.DefaultBillingAddressId);

        builder.Property(p => p.DefaultShippingAddressId);

        builder.HasOne<Address>()
            .WithMany()
            .HasForeignKey(p => p.DefaultBillingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Address>()
            .WithMany()
            .HasForeignKey(p => p.DefaultShippingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(p => p.Preferences, preferences =>
        {
            preferences.Property(up => up.PreferredStyle)
                .HasMaxLength(UserPreferenceConstant.Constraints.MaxPreferredStyleLength);

            preferences.Property(up => up.PreferredFit)
                .HasMaxLength(UserPreferenceConstant.Constraints.MaxPreferredFitLength);

            preferences.Property(up => up.SizeTop)
                .HasMaxLength(UserPreferenceConstant.Constraints.MaxSizeTopLength);

            preferences.Property(up => up.SizeBottom)
                .HasMaxLength(UserPreferenceConstant.Constraints.MaxSizeBottomLength);

            preferences.Property(up => up.ShoeSize)
                .HasMaxLength(UserPreferenceConstant.Constraints.MaxShoeSizeLength);

            preferences.Property(up => up.FavoriteColors)
                .HasConversion<string>();

            preferences.Property(up => up.FavoriteCategories)
                .HasConversion<string>();

            preferences.Property(up => up.PreferredBrands)
                .HasConversion<string>();
        });

        builder.OwnsOne(p => p.Notifications);

        builder.HasMany(p => p.Addresses)
            .WithOne(p => p.UserProfile)
            .HasForeignKey(p => p.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}