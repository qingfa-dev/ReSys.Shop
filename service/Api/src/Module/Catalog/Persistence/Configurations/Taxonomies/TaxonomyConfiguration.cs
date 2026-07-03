using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Taxonomies;

namespace Module.Catalog.Persistence.Configurations.Taxonomies;

public class TaxonomyConfiguration : IEntityTypeConfiguration<Taxonomy>
{
    public void Configure(EntityTypeBuilder<Taxonomy> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.Taxonomies, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(TaxonomyConstant.Constraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(TaxonomyConstant.Constraints.PresentationMaxLength);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(0);
        #endregion

        #region Relationships
        builder.HasMany(x => x.Taxons)
            .WithOne(v => v.Taxonomy)
            .HasForeignKey(v => v.TaxonomyId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}
