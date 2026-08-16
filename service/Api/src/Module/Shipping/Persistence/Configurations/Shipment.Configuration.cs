using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Shipping.Persistence.Constants;
using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable(ShippingSchema.TableNames.Shipments, ShippingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ShippingMethodId).IsRequired();
        builder.Property(x => x.TrackingNumber).HasMaxLength(ShipmentConstant.Constraints.MaxTrackingLength);
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ShipmentStatus.Pending);
        #endregion Properties

        #region Relationships
        builder.HasOne(x => x.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Address)
            .WithMany(a => a.Shipments)
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ShippingMethod)
            .WithMany(sm => sm.Shipments)
            .HasForeignKey(x => x.ShippingMethodId)
            .OnDelete(DeleteBehavior.Restrict);
        #endregion Relationships

        #region Indexes
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.OrderId, x.Status });
        #endregion Indexes
    }
}
