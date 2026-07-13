using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Configurations.Taxonomies;

public class TaxonConfiguration : IEntityTypeConfiguration<Taxon>
{
    public void Configure(EntityTypeBuilder<Taxon> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.Taxa, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.PresentationMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(TaxonConstant.Constraints.DescriptionMaxLength);

        builder.Property(x => x.Position)
            .HasDefaultValue(TaxonConstant.Default.Position);

        builder.Property(x => x.HideFromNav)
            .HasDefaultValue(false);

        builder.Property(x => x.Permalink)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.PermalinkMaxLength);

        builder.Property(x => x.PrettyName)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.PrettyNameMaxLength);

        builder.Property(x => x.Depth)
            .IsRequired()
            .HasDefaultValue(TaxonConstant.Default.Position);

        builder.Property(x => x.Lft)
            .IsRequired()
            .HasDefaultValue(TaxonConstant.Default.Position);

        builder.Property(x => x.Rgt)
            .IsRequired()
            .HasDefaultValue(TaxonConstant.Default.Position);
        #endregion

        #region Settings
        builder.Property(x => x.Automatic)
            .HasDefaultValue(false);

        builder.Property(x => x.RulesMatchPolicy)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.PolicyMaxLength)
            .HasConversion<string>()
            .HasDefaultValue(TaxonMatchPolicy.All);

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.SortOrderMaxLength)
            .HasConversion<string>()
            .HasDefaultValue(TaxonSortOrder.Manual);
        #endregion

        #region Automatic
        builder.Property(x => x.MarkedForRegenerateTaxonProducts)
            .HasDefaultValue(false);
        #endregion

        #region Images
        builder.Property(x => x.ImageUrl)
            .HasMaxLength(TaxonConstant.Constraints.UrlMaxLength);

        builder.Property(x => x.SquareImageUrl)
            .HasMaxLength(TaxonConstant.Constraints.UrlMaxLength);
        #endregion

        #region SEO
        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(TaxonConstant.Constraints.SlugMaxLength);

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(TaxonConstant.Constraints.MetaTitleMaxLength);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(TaxonConstant.Constraints.MetaDescriptionMaxLength);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(TaxonConstant.Constraints.MetaKeywordsMaxLength);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Taxonomy)
            .WithMany(t => t.Taxons)
            .HasForeignKey(x => x.TaxonomyId);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TaxonRules)
            .WithOne(r => r.Taxon)
            .HasForeignKey(r => r.TaxonId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}