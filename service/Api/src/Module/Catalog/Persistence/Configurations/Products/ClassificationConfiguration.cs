using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products.Classifications;

namespace Module.Catalog.Persistence.Configurations.Products;

public class ClassificationConfiguration : IEntityTypeConfiguration<Classification>
{
    public void Configure(EntityTypeBuilder<Classification> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.Classifications, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(ClassificationConstant.Defaults.Position);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Classifications)
            .HasForeignKey(x => x.ProductId);

        builder.HasOne(x => x.Taxon)
            .WithMany(t => t.Classifications)
            .HasForeignKey(x => x.TaxonId);
        #endregion

        #region Indexes
        builder.HasIndex(x => new { x.ProductId, x.TaxonId }).IsUnique();
        #endregion
    }
}