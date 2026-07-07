using Microsoft.EntityFrameworkCore;
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
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(ShipmentConstant.Constraints.MaxNumberLength);

        builder.Property(x => x.State)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ShipmentConstant.Defaults.State);

        builder.Property(x => x.Tracking)
            .HasMaxLength(ShipmentConstant.Constraints.MaxTrackingLength);

        builder.Property(x => x.Cost)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.DiscountedCost)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.FinalPrice)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.ItemCost)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.AdditionalTaxTotal)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.IncludedTaxTotal)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.TaxTotal)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.PromoTotal)
            .HasPrecision(ShipmentConstant.Constraints.Precision, ShipmentConstant.Constraints.Scale);

        builder.Property(x => x.ShippedAtUtc);

        builder.Property(x => x.OrderId);
        builder.Property(x => x.StockLocationId);
        builder.Property(x => x.ShippingMethodId);
        builder.Property(x => x.AddressId);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StockLocation)
            .WithMany(sl => sl.Shipments)
            .HasForeignKey(x => x.StockLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Address)
            .WithMany()
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.ShippingRates)
            .WithOne(sr => sr.Shipment)
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}
