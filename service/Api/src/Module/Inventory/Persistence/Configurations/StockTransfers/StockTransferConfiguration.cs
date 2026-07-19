using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockTransfers;

namespace Module.Inventory.Persistence.Configurations.StockTransfers;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable(InventorySchema.TableNames.StockTransfers, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(StockTransferConstant.Constraints.NumberMaxLength);

        builder.Property(x => x.Reference)
            .HasMaxLength(StockTransferConstant.Constraints.ReferenceMaxLength);

        builder.Property(x => x.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.SourceLocationId)
            .IsRequired();

        builder.Property(x => x.DestinationLocationId)
            .IsRequired();
        #endregion

        #region Relationships
        builder.HasOne(x => x.SourceLocation)
            .WithMany()
            .HasForeignKey(x => x.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationLocation)
            .WithMany()
            .HasForeignKey(x => x.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TransferItems)
            .WithOne()
            .HasForeignKey(ti => ti.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        #region Indexes
        builder.HasIndex(x => x.State);
        builder.HasIndex(x => x.SourceLocationId);
        builder.HasIndex(x => x.DestinationLocationId);
        #endregion
    }
}