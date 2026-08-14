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
            .HasConversion(
                v => v.ToString(),
                v => v switch
                {
                    "Delivery" => CheckoutState.PickDeliveryMethod,
                    "Payment"  => CheckoutState.PickPaymentMethod,
                    _ => Enum.Parse<CheckoutState>(v)
                })
            .HasDefaultValue(CheckoutState.Address);

        builder.Property(x => x.PaymentState)
            .HasConversion(
                v => v!.ToString(),
                v => v switch
                {
                    "completed"   => OrderPaymentState.Completed,
                    "failed"      => OrderPaymentState.Failed,
                    "void"        => OrderPaymentState.Void,
                    "balance_due" => OrderPaymentState.BalanceDue,
                    "credit_owed" => OrderPaymentState.CreditOwed,
                    "paid"        => OrderPaymentState.Paid,
                    "pending"     => OrderPaymentState.Pending,
                    "checkout"    => OrderPaymentState.Checkout,
                    "invalid"     => OrderPaymentState.Invalid,
                    _ => Enum.Parse<OrderPaymentState>(v)
                });

        builder.Property(x => x.ShipmentState)
            .HasConversion(
                v => v!.ToString(),
                v => v switch
                {
                    "pending"   => OrderShipmentState.Pending,
                    "delivered" => OrderShipmentState.Delivered,
                    "partial"   => OrderShipmentState.Partial,
                    "ready"     => OrderShipmentState.Ready,
                    "backorder" => OrderShipmentState.Backorder,
                    "canceled"  => OrderShipmentState.Canceled,
                    _ => Enum.Parse<OrderShipmentState>(v)
                });

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
            .WithOne()
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Indexes
        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => new { x.SessionId, x.Status });
        #endregion
    }
}