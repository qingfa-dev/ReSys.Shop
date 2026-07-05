using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Persistence.Configurations.StockReservations;

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
        builder.Property(x => x.Reason);
        #endregion
    }
}
