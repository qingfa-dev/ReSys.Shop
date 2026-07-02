using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Profile.Domain.Wishlists.WishedItems;
using Module.Profile.Persistence.Constants;

namespace Module.Profile.Persistence.Configurations;

public sealed class WishedItemConfiguration : IEntityTypeConfiguration<WishedItem>
{
    public void Configure(EntityTypeBuilder<WishedItem> builder)
    {
        builder.ToTable(ProfileSchema.TableNames.WishedItems, ProfileSchema.Name);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.WishlistId).IsRequired();

        builder.HasOne(x => x.Wishlist)
            .WithMany(w => w.WishedItems)
            .HasForeignKey(x => x.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.WishlistId, x.VariantId }).IsUnique();
    }
}