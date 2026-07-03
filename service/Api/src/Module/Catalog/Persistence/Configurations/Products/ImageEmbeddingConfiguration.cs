using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Persistence.Configurations.Products;

public class ImageEmbeddingConfiguration : IEntityTypeConfiguration<ImageEmbedding>
{
    public void Configure(EntityTypeBuilder<ImageEmbedding> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.ProductImageEmbeddings, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Vector)
            .IsRequired();

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(ImageEmbeddingConstant.Constraints.ModelNameMaxLength);

        builder.Property(x => x.ModelVersion)
            .HasMaxLength(ImageEmbeddingConstant.Constraints.ModelVersionMaxLength);

        builder.Property(x => x.Dimensions)
            .IsRequired();
        #endregion

        #region Relationships
        builder.HasOne(x => x.VariantImage)
            .WithMany(i => i.ImageEmbedding)
            .HasForeignKey(x => x.VariantImageId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}
