using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Shipping.Persistence.Constants;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Persistence.Configurations.ShippingMethods;

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable(ShippingSchema.TableNames.ShippingMethods, ShippingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ShippingMethodConstant.Constraints.MaxNameLength);

        builder.Property(x => x.Code)
            .HasMaxLength(ShippingMethodConstant.Constraints.MaxCodeLength);

        builder.Property(x => x.TrackingUrl)
            .HasMaxLength(ShippingMethodConstant.Constraints.MaxTrackingUrlLength);

        builder.Property(x => x.AdminName)
            .HasMaxLength(ShippingMethodConstant.Constraints.MaxAdminNameLength);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(ShippingMethodConstant.Defaults.Position);

        builder.Property(x => x.AvailableToUsers)
            .IsRequired()
            .HasDefaultValue(ShippingMethodConstant.Defaults.AvailableToUsers);

        builder.Property(x => x.CalculatorType)
            .IsRequired()
            .HasMaxLength(ShippingMethodConstant.Constraints.MaxCalculatorTypeLength);

        builder.Property(x => x.TaxCategoryId);
        builder.Property(x => x.Presentation);
        #endregion

        #region Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        #endregion
    }
}
