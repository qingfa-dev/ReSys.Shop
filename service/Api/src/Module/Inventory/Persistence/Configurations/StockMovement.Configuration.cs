using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockMovements;

namespace Module.Inventory.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable(InventorySchema.TableNames.StockMovements, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(StockMovementConstant.Constraints.MaxActionLength);
        builder.Property(x => x.OriginatorType).HasMaxLength(StockMovementConstant.Constraints.MaxOriginatorTypeLength);
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