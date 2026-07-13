using Microsoft.EntityFrameworkCore.Metadata.Builders;


using Module.Catalog.Domain.Products.Variants.Images;

namespace Module.Catalog.Persistence.Configurations.Products;

public class VariantImageConfiguration : IEntityTypeConfiguration<VariantImage>
{
    public void Configure(EntityTypeBuilder<VariantImage> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.ProductImages, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(VariantImageConstant.Constraints.UrlMaxLength);

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(VariantImageConstant.Constraints.StoragePathMaxLength);

        builder.Property(x => x.Alt)
            .HasMaxLength(VariantImageConstant.Constraints.AltMaxLength)
            .HasDefaultValue(VariantImageConstant.Defaults.DefaultImageAlt);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(VariantImageConstant.Constraints.ContentTypeMaxLength)
            .HasDefaultValue(VariantImageConstant.Defaults.DefaultContentType);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(VariantImageConstant.Constraints.FileNameMaxLength);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Width);
        builder.Property(x => x.Height);
        builder.Property(x => x.DimensionsUnit)
            .HasMaxLength(VariantImageConstant.Constraints.DimensionsUnitMaxLength)
            .HasDefaultValue(VariantImageConstant.Defaults.DimensionsUnit);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(VariantImageConstant.Defaults.Position);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(VariantImageConstant.Constraints.TypeMaxLength)
            .HasDefaultValue(VariantImageType.Default);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.VariantImages)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ImageEmbedding)
            .WithOne(e => e.VariantImage)
            .HasForeignKey(e => e.VariantImageId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}