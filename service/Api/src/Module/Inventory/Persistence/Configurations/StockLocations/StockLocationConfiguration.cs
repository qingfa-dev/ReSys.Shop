using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Inventory.Persistence.Constants;
using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Persistence.Configurations.StockLocations;

public class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        builder.ToTable(InventorySchema.TableNames.StockLocations, InventorySchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(StockLocationConstant.Constraints.NameMaxLength);

        builder.Property(x => x.Code)
            .HasMaxLength(StockLocationConstant.Constraints.CodeMaxLength);

        builder.Property(x => x.Active)
            .IsRequired()
            .HasDefaultValue(StockLocationConstant.Defaults.Active);

        builder.Property(x => x.AdminName)
            .HasMaxLength(StockLocationConstant.Constraints.AdminNameMaxLength);

        builder.Property(x => x.Presentation)
            .HasMaxLength(StockLocationConstant.Constraints.PresentationMaxLength);

        builder.Property(x => x.Address1)
            .HasMaxLength(StockLocationConstant.Constraints.AddressMaxLength);

        builder.Property(x => x.Address2)
            .HasMaxLength(StockLocationConstant.Constraints.AddressMaxLength);

        builder.Property(x => x.City)
            .HasMaxLength(StockLocationConstant.Constraints.CityMaxLength);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(StockLocationConstant.Constraints.PostalCodeMaxLength);

        builder.Property(x => x.Phone)
            .HasMaxLength(StockLocationConstant.Constraints.PhoneMaxLength);

        builder.Property(x => x.Default)
            .HasDefaultValue(StockLocationConstant.Defaults.Default);

        builder.Property(x => x.BackorderableDefault)
            .HasDefaultValue(StockLocationConstant.Defaults.BackorderableDefault);

        builder.Property(x => x.PropagateAllVariants)
            .HasDefaultValue(StockLocationConstant.Defaults.PropagateAllVariants);

        builder.Property(x => x.Position)
            .HasDefaultValue(StockLocationConstant.Defaults.Position);

        builder.Property(x => x.LowStockThreshold)
            .IsRequired()
            .HasDefaultValue(StockLocationConstant.Defaults.LowStockThreshold);

        builder.Property(x => x.NotifyOnLowStock)
            .IsRequired()
            .HasDefaultValue(StockLocationConstant.Defaults.NotifyOnLowStock);

        builder.Property(x => x.CountryId);
        builder.Property(x => x.StateId);
        #endregion

        #region Relationships
        builder.HasMany(x => x.StockItems)
            .WithOne(si => si.StockLocation)
            .HasForeignKey(si => si.StockLocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StockMovements)
            .WithOne(sm => sm.StockLocation)
            .HasForeignKey(sm => sm.StockLocationId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}