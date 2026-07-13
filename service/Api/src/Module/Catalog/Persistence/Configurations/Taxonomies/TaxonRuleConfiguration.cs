using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Taxonomies.Taxons.Rules;

namespace Module.Catalog.Persistence.Configurations.Taxonomies;

public class TaxonRuleConfiguration : IEntityTypeConfiguration<TaxonRule>
{
    public void Configure(EntityTypeBuilder<TaxonRule> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.TaxonRules, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(TaxonRuleConstant.Constraints.TypeMaxLength)
            .HasConversion<string>();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(TaxonRuleConstant.Constraints.ValueMaxLength);

        builder.Property(x => x.MatchPolicy)
            .IsRequired()
            .HasMaxLength(TaxonRuleConstant.Constraints.PolicyMaxLength)
            .HasConversion<string>();
        #endregion

        #region Relationships
        builder.HasOne(x => x.Taxon)
            .WithMany(t => t.TaxonRules)
            .HasForeignKey(x => x.TaxonId);
        #endregion
    }
}