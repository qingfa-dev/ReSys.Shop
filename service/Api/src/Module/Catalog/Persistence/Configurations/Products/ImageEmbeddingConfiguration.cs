using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Variants.Images.Embeddings;

using Shared.Operational.Persistence.Configurations.Vectors;

namespace Module.Catalog.Persistence.Configurations.Products;

public class ImageEmbeddingConfiguration : IEntityTypeConfiguration<ImageEmbedding>
{
    public void Configure(EntityTypeBuilder<ImageEmbedding> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.VariantImageEmbeddings, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        // Untyped vector column — supports different dimensions per model.
        // Per-model HNSW indexes are created via raw SQL in DatabaseInitializer
        // because pgvector requires expression indexes with dimension casts
        // (::vector(dim)) which EF Core cannot generate.
        builder.Property(x => x.Vector)
            .HasColumnType("vector");

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
        builder.HasIndex(x => x.ModelName)
            .HasDatabaseName("ix_product_image_embeddings_model_name");

        // Per-model HNSW partial indexes with expression casts (::vector(dim))
        // are created by DatabaseInitializer.EnsureVectorIndexesAsync.
        // EF Core can't generate expression indexes, and pgvector rejects
        // plain HNSW on untyped vector columns (no dimensions).
        #endregion

        #region Relationships
        builder.HasOne(x => x.VariantImage)
            .WithMany(i => i.ImageEmbeddings)
            .HasForeignKey(x => x.VariantImageId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}