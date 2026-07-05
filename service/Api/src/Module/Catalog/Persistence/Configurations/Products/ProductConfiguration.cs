using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products;

namespace Module.Catalog.Persistence.Configurations.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.Products, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ProductConstant.Constraints.MaxNameLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(ProductConstant.Constraints.MaxSlugLength);

        builder.Property(x => x.Description)
            .HasMaxLength(ProductConstant.Constraints.MaxDescriptionLength);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ProductConstant.Defaults.Status);

        builder.Property(x => x.AvailableOn);
        builder.Property(x => x.DiscontinueOn);
        builder.Property(x => x.MakeActiveAt);

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(ProductConstant.Constraints.MaxMetaTitleLength);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(ProductConstant.Constraints.MaxMetaDescriptionLength);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(ProductConstant.Constraints.MaxMetaKeywordsLength);

        #endregion

        #region Relationships
        builder.HasMany(x => x.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProductOptionTypes)
            .WithOne(po => po.Product)
            .HasForeignKey(po => po.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Classifications)
            .WithOne(c => c.Product)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Indexes
        builder.HasIndex(x => x.Slug).IsUnique();
        #endregion
    }
}
