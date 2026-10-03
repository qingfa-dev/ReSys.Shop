using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Shipping.Persistence.Constants;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Persistence.Configurations;

public class ShippingMethodZoneConfiguration : IEntityTypeConfiguration<ShippingMethodZone>
{
    public void Configure(EntityTypeBuilder<ShippingMethodZone> builder)
    {
        builder.ToTable(ShippingSchema.TableNames.ShippingMethodZones, ShippingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.CountryCode)
            .IsRequired()
            .HasMaxLength(ShippingMethodZoneConstant.Constraints.MaxCountryCodeLength);

        builder.Property(x => x.StateCode)
            .HasMaxLength(ShippingMethodZoneConstant.Constraints.MaxStateCodeLength);
        #endregion

        #region Relationships
        builder.HasOne(x => x.ShippingMethod)
            .WithMany(sm => sm.ShippingMethodZones)
            .HasForeignKey(x => x.ShippingMethodId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Indexes
        builder.HasIndex(x => new { x.ShippingMethodId, x.CountryCode, x.StateCode });
        #endregion
    }
}
