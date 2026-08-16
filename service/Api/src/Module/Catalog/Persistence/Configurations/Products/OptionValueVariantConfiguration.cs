using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Variants.Options;

namespace Module.Catalog.Persistence.Configurations.Products;

public class OptionValueVariantConfiguration : IEntityTypeConfiguration<OptionValueVariant>
{
    public void Configure(EntityTypeBuilder<OptionValueVariant> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.OptionValueVariants, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.OptionValueVariants)
            .HasForeignKey(x => x.VariantId);

        builder.HasOne(x => x.OptionValue)
            .WithMany(ov => ov.OptionValueVariants)
            .HasForeignKey(x => x.OptionValueId);
        #endregion

        #region Indexes
        builder.HasIndex(x => new { x.VariantId, x.OptionValueId }).IsUnique();
        #endregion
    }
}