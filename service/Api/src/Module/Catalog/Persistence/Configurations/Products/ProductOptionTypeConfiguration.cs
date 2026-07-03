using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products.Options;

namespace Module.Catalog.Persistence.Configurations.Products;

public class ProductOptionTypeConfiguration : IEntityTypeConfiguration<ProductOptionType>
{
    public void Configure(EntityTypeBuilder<ProductOptionType> builder)
    {
        builder.ToTable("product_option_types", CatalogSchema.Name); // Join table for Product <-> OptionType

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(ProductOptionTypeConstant.Defaults.Position);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.ProductOptionTypes)
            .HasForeignKey(x => x.ProductId);

        builder.HasOne(x => x.OptionType)
            .WithMany(o => o.ProductOptionTypes)
            .HasForeignKey(x => x.OptionTypeId);

        #endregion

        #region Indexes
        builder.HasIndex(x => new { x.ProductId, x.OptionTypeId }).IsUnique();
        #endregion
    }
}
