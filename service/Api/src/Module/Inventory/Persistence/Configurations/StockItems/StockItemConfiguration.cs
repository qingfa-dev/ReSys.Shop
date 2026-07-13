using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Persistence.Configurations.StockItems;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable(InventorySchema.TableNames.StockItems, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.CountOnHand)
            .IsRequired()
            .HasDefaultValue(StockItemConstant.Defaults.CountOnHand);

        builder.Property(x => x.Backorderable)
            .IsRequired()
            .HasDefaultValue(StockItemConstant.Defaults.Backorderable);

        builder.Property(x => x.StockLocationId).IsRequired();
        builder.Property(x => x.VariantId).IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
        #endregion

        #region Relationships
        builder.HasMany(x => x.StockMovements)
            .WithOne(sm => sm.StockItem)
            .HasForeignKey(sm => sm.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Indexes
        builder.HasIndex(x => new { x.StockLocationId, x.VariantId }).IsUnique();
        #endregion
    }
}