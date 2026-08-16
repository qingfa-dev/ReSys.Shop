using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Customer.Domain.Wishlists;

namespace Module.Customer.Persistence.Configurations;

public sealed class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable(ProfileSchema.TableNames.Wishlists, ProfileSchema.Name);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(WishlistConstant.Constraints.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasMaxLength(WishlistConstant.Constraints.MaxTokenLength)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(WishlistConstant.Defaults.IsDefault);

        builder.Property(x => x.IsPrivate)
            .IsRequired()
            .HasDefaultValue(WishlistConstant.Defaults.IsPrivate);

        builder.Property(x => x.UserId).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.IsDefault });
        builder.HasIndex(x => x.Token).IsUnique();
    }
}