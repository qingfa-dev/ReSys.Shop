using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable(InventorySchema.TableNames.StockReservations, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.ExpiresAtUtc);
        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.StockLocationId);
        builder.Property(x => x.OrderId);
        builder.Property(x => x.State).HasConversion<string>();
        builder.Property(x => x.Reason).HasMaxLength(StockReservationConstant.Constraints.MaxReasonLength);
        builder.Property(x => x.RowVersion)
            .IsRowVersion();
        #endregion

        #region Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.StockReservations)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StockLocation)
            .WithMany(sl => sl.StockReservations)
            .HasForeignKey(x => x.StockLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Order)
            .WithMany(o => o.StockReservations)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        #endregion

        #region Indexes
        builder.HasIndex(x => new { x.OrderId, x.State });
        builder.HasIndex(x => new { x.CartToken, x.State });
        #endregion
    }
}