using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.Inventory.Persistence.Configurations.StockMovements;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable(InventorySchema.TableNames.StockMovements, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(50);
        builder.Property(x => x.OriginatorType).HasMaxLength(200);
        builder.Property(x => x.OriginatorId);
        builder.Property(x => x.StockItemId).IsRequired();
        builder.Property(x => x.StockLocationId);
        builder.Property(x => x.Reason).HasMaxLength(StockMovementConstant.Constraints.MaxReasonLength);
        #endregion

        #region Relationships
        builder.HasOne(x => x.StockItem)
            .WithMany(si => si.StockMovements)
            .HasForeignKey(x => x.StockItemId);

        builder.HasOne(x => x.StockLocation)
            .WithMany(sl => sl.StockMovements)
            .HasForeignKey(x => x.StockLocationId);
        #endregion
    }
}
