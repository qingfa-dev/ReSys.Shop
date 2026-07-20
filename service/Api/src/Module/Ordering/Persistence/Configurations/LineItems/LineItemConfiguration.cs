using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.Products.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Persistence.Configurations.LineItems;

// @CAT-10 Boundary: Persistence → Domain — reserved for EF Core materialization; do not add domain logic
public class LineItemConfiguration : IEntityTypeConfiguration<LineItem>
{
    public void Configure(EntityTypeBuilder<LineItem> builder)
    {
        builder.ToTable(OrderingSchema.TableNames.LineItems, OrderingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        // Initialize: Quantity defaults to 1; monetary fields use shared precision/scale constants
        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(LineItemConstant.Defaults.Quantity);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(LineItemConstant.Precision, LineItemConstant.Scale);

        builder.Property(x => x.Total)
            .HasPrecision(LineItemConstant.Precision, LineItemConstant.Scale);

        builder.Property(x => x.AdjustmentTotal)
            .HasPrecision(LineItemConstant.Precision, LineItemConstant.Scale);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(OrderConstant.Constraints.MaxCurrencyLength)
            .HasDefaultValue(OrderConstant.Defaults.Currency);

        builder.Property(x => x.VariantId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
        #endregion

        #region Relationships
        builder.HasOne(x => x.Order)
            .WithMany(o => o.LineItems)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Variant>()
            .WithMany()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.NoAction);
        #endregion
    }
}