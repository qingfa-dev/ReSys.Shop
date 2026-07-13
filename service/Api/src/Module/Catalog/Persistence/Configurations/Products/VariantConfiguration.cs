using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products.Variants;

namespace Module.Catalog.Persistence.Configurations.Products;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.Variants, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(VariantConstant.Constraints.SkuMaxLength);

        builder.Property(x => x.IsMaster)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(VariantConstant.Defaults.Position);

        builder.Property(x => x.TrackInventory)
            .IsRequired()
            .HasDefaultValue(VariantConstant.Defaults.TrackInventory);

        builder.Property(x => x.Price)
            .HasPrecision(VariantConstant.Constraints.Price.Precision, VariantConstant.Constraints.Price.Scale)
            .HasDefaultValue(VariantConstant.Defaults.Price);

        builder.Property(x => x.CostPrice)
            .HasPrecision(VariantConstant.Constraints.Price.Precision, VariantConstant.Constraints.Price.Scale)
            .HasDefaultValue(VariantConstant.Defaults.CostPrice);

        builder.Property(x => x.CostCurrency)
            .HasMaxLength(VariantConstant.Constraints.Price.CurrencyMaxLength)
            .HasDefaultValue(VariantConstant.Defaults.CostCurrency);

        builder.Property(x => x.Weight)
            .HasPrecision(VariantConstant.Constraints.Weight.Precision, VariantConstant.Constraints.Weight.Scale)
            .HasDefaultValue(VariantConstant.Defaults.Weight);

        builder.Property(x => x.WeightUnit)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(WeightUnit.Kg)
            .IsRequired(false);

        builder.Property(x => x.Height)
            .HasPrecision(VariantConstant.Constraints.Weight.Precision, VariantConstant.Constraints.Weight.Scale)
            .HasDefaultValue(VariantConstant.Defaults.Height);

        builder.Property(x => x.Width)
            .HasPrecision(VariantConstant.Constraints.Weight.Precision, VariantConstant.Constraints.Weight.Scale)
            .HasDefaultValue(VariantConstant.Defaults.Width);

        builder.Property(x => x.Depth)
            .HasPrecision(VariantConstant.Constraints.Weight.Precision, VariantConstant.Constraints.Weight.Scale)
            .HasDefaultValue(VariantConstant.Defaults.Depth);

        builder.Property(x => x.DimensionsUnit)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(DimensionUnit.Cm)
            .IsRequired(false);

        builder.Property(x => x.DiscontinuedOn);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Prices)
            .WithOne(p => p.Variant)
            .HasForeignKey(p => p.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.VariantImages)
            .WithOne(i => i.Variant)
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OptionValueVariants)
            .WithOne(ovv => ovv.Variant)
            .HasForeignKey(ovv => ovv.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Indexes
        builder.HasIndex(x => x.Sku).IsUnique();
        #endregion
    }
}