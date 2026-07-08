using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Shipping.Persistence.Constants;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Persistence.Configurations.ShippingRates;

public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
{
    public void Configure(EntityTypeBuilder<ShippingRate> builder)
    {
        builder.ToTable(ShippingSchema.TableNames.ShippingRates, ShippingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ShippingRateConstant.Constraints.MaxNameLength);

        builder.Property(x => x.Selected)
            .IsRequired()
            .HasDefaultValue(ShippingRateConstant.Defaults.Selected);

        builder.Property(x => x.Cost)
            .HasPrecision(ShippingRateConstant.Constraints.Precision, ShippingRateConstant.Constraints.Scale);

        builder.Property(x => x.FinalPrice)
            .HasPrecision(ShippingRateConstant.Constraints.Precision, ShippingRateConstant.Constraints.Scale);

        builder.Property(x => x.DisplayPrice)
            .IsRequired()
            .HasMaxLength(ShippingRateConstant.Constraints.MaxDisplayPriceLength);

        builder.Property(x => x.DeliveryRange)
            .HasMaxLength(ShippingRateConstant.Constraints.MaxDeliveryRangeLength);

        builder.Property(x => x.MinWeight)
            .HasPrecision(ShippingRateConstant.Constraints.Precision, ShippingRateConstant.Constraints.Scale);

        builder.Property(x => x.MaxWeight)
            .HasPrecision(ShippingRateConstant.Constraints.Precision, ShippingRateConstant.Constraints.Scale);

        builder.Property(x => x.FreeShippingThreshold)
            .HasPrecision(ShippingRateConstant.Constraints.Precision, ShippingRateConstant.Constraints.Scale);

        builder.Property(x => x.ShippingMethodId);
        #endregion

        #region Indexes
        #endregion
    }
}
