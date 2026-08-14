using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Shipping.Persistence.Constants;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Persistence.Configurations.Shipments;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable(ShippingSchema.TableNames.Shipments, ShippingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ShippingMethodId).IsRequired();
        builder.Property(x => x.TrackingNumber).HasMaxLength(200);
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ShipmentStatus.Pending);
        #endregion Properties

        #region Indexes
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.OrderId, x.Status });
        #endregion Indexes
    }
}
