using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Variants.Prices;

namespace Module.Catalog.Persistence.Configurations.Products;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.Prices, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(PriceConstant.Constraints.Precision, PriceConstant.Constraints.Scale)
            .HasDefaultValue(0m);

        builder.Property(x => x.CompareAtAmount)
            .HasPrecision(PriceConstant.Constraints.Precision, PriceConstant.Constraints.Scale);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(PriceConstant.Constraints.CurrencyMaxLength)
            .HasDefaultValue(PriceConstant.Default.Currency);

        builder.Property(x => x.CountryIso)
            .HasMaxLength(PriceConstant.Constraints.CountryIsoMaxLength);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.Prices)
            .HasForeignKey(x => x.VariantId);
        #endregion
    }
}