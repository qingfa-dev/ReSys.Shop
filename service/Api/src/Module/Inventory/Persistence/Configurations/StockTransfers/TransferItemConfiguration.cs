using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Persistence.Configurations.StockTransfers;

public class TransferItemConfiguration : IEntityTypeConfiguration<TransferItem>
{
    public void Configure(EntityTypeBuilder<TransferItem> builder)
    {
        builder.ToTable(InventorySchema.TableNames.TransferItems, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StockTransferId)
            .IsRequired();

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.ReceivedQuantity)
            .IsRequired()
            .HasDefaultValue(0);
    }
}
