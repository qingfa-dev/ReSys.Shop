using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Customer.Domain.Wishlists.WishedItems;

namespace Module.Customer.Persistence.Configurations;

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

        builder.HasOne(x => x.Variant)
            .WithMany(v => v.WishedItems)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.WishlistId, x.VariantId }).IsUnique();
    }
}