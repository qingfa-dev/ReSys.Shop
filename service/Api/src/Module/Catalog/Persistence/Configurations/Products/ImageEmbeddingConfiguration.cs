using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

using Shared.Operational.Persistence.Configurations.Vectors;

namespace Module.Catalog.Persistence.Configurations.Products;

public class ImageEmbeddingConfiguration : IEntityTypeConfiguration<ImageEmbedding>
{
    public void Configure(EntityTypeBuilder<ImageEmbedding> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.VariantImageEmbeddings, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Vector)
            .HasColumnType("vector(512)");

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(ImageEmbeddingConstant.Constraints.ModelNameMaxLength);

        builder.Property(x => x.ModelVersion)
            .HasMaxLength(ImageEmbeddingConstant.Constraints.ModelVersionMaxLength);

        builder.Property(x => x.Dimensions)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(EmbeddingStatus.Completed);
        #endregion

        #region Indexes
        builder.ConfigureIVFFlatIndex(
            x => x.Vector,
            lists: 100,
            indexName: "ix_product_image_embeddings_vector_ivfflat");
        #endregion

        #region Relationships
        builder.HasOne(x => x.VariantImage)
            .WithMany(i => i.ImageEmbeddings)
            .HasForeignKey(x => x.VariantImageId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}