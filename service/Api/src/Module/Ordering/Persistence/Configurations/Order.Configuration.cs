using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Persistence.Configurations.Orders;

// @CAT-10 Boundary: Persistence → Domain — reserved for EF Core materialization; do not add domain logic
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(OrderingSchema.TableNames.Orders, OrderingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        // Initialize: OrderNumber with unique constraint — used as human-readable identifier in UI and invoices
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(OrderConstant.Constraints.MaxNumberLength);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(OrderStatus.Draft);

        builder.Property(x => x.CheckoutState)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(CheckoutState.Address);

        builder.Property(x => x.PaymentState)
            .HasConversion<string>();

        builder.Property(x => x.ShipmentState)
            .HasConversion<string>();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(OrderConstant.Constraints.MaxCurrencyLength)
            .HasDefaultValue(OrderConstant.Defaults.Currency);

        builder.Property(x => x.ItemTotal)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.AdjustmentTotal)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.ShipmentTotal)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.Total)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.PaymentTotal)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.OutstandingBalance)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.TotalWeight)
            .HasPrecision(OrderConstant.Constraints.Precision, OrderConstant.Constraints.Scale);

        builder.Property(x => x.ShippingRateId);

        builder.Property(x => x.IsFreeShipping)
            .HasDefaultValue(false);

        builder.Property(x => x.Email)
            .HasMaxLength(OrderConstant.Constraints.MaxEmailLength);

        builder.Property(x => x.SpecialInstructions)
            .HasMaxLength(OrderConstant.Constraints.MaxSpecialInstructionsLength);

        builder.Property(x => x.SessionId)
            .HasMaxLength(OrderConstant.Constraints.MaxSessionIdLength);

        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.CanceledAtUtc);
        builder.Property(x => x.CanceledById);
        builder.Property(x => x.ApprovedById);
        builder.Property(x => x.BillAddressId);
        builder.Property(x => x.ShipAddressId);
        builder.Property(x => x.UserId);
        builder.Property(x => x.ShippingMethodId);
        #endregion

        #region Relationships
        builder.HasMany(x => x.Adjustments)
            .WithOne(x => x.Order)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany(pm => pm.Orders)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ShippingMethod)
            .WithMany(sm => sm.Orders)
            .HasForeignKey(x => x.ShippingMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BillAddress)
            .WithMany(a => a.BillingOrders)
            .HasForeignKey(x => x.BillAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ShipAddress)
            .WithMany(a => a.ShippingOrders)
            .HasForeignKey(x => x.ShipAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ShippingRate)
            .WithMany(sr => sr.Orders)
            .HasForeignKey(x => x.ShippingRateId)
            .OnDelete(DeleteBehavior.Restrict);
        #endregion

        #region Indexes
        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => new { x.SessionId, x.Status });
        builder.HasIndex(x => x.BillAddressId);
        builder.HasIndex(x => x.ShipAddressId);
        builder.HasIndex(x => x.ShippingRateId);
        #endregion
    }
}