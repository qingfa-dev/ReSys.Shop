using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Models;

using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;
using Module.Shipping.Domain.ShippingMethods;
namespace Module.Ordering.Domain.Orders;

/// <summary>
/// Represents a customer order as the aggregate root in the ordering context.
/// </summary>
// @CAT-10 Invariant: Total = ItemTotal + AdjustmentTotal + ShipmentTotal; CheckoutState progresses forward; Finalized orders are immutable except Cancel; Total >= 0
// @CAT-10 Boundary: Domain → Persistence — EF Core entity; do not add persistence concerns to domain logic
public sealed partial class Order : Entity, IAuditable, ISoftDeletable
{
    #region Properties
    public string Number { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public CheckoutState CheckoutState { get; set; } = CheckoutState.Address;
    public string Currency { get; set; } = OrderConstant.Defaults.Currency;
    public decimal ItemTotal { get; set; }
    public decimal AdjustmentTotal { get; set; }
    public decimal ShipmentTotal { get; set; }
    public decimal Total { get; set; }
    public decimal PaymentTotal { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? PaymentState { get; set; }
    public string? ShipmentState { get; set; }
    #endregion Properties

    #region Contact
    public string? Email { get; set; }
    public string? SpecialInstructions { get; set; }
    #endregion Contact

    #region Timestamps
    #region Payment State
    public DateTimeOffset? PaymentProcessingAt { get; set; }
    public DateTimeOffset? PaymentCompletedAt { get; set; }
    public DateTimeOffset? PaymentFailedAt { get; set; }
    #endregion

    #region Order State
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? CanceledAtUtc { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? DeliveryExceptionAt { get; set; }
    public DateTimeOffset? EstimatedDeliveryAt { get; set; }
    #endregion

    #endregion Timestamps

    #region Approval & Cancel
    public Guid? CanceledById { get; set; }
    public Guid? ApprovedById { get; set; }
    #endregion Approval & Cancel

    #region Addresses
    public Guid? BillAddressId { get; set; }
    public Guid? ShipAddressId { get; set; }
    #endregion Addresses

    #region Relationships
    public Guid? UserId { get; set; }
    public Guid? ShippingMethodId { get; set; }
    #endregion Relationships

    #region Navigation
    public ShippingMethod? ShippingMethod { get; set; }
    public ICollection<LineItem> LineItems { get; set; } = [];
    public ICollection<Adjustment> Adjustments { get; set; } = [];
    #endregion Navigation

    #region Soft Deletion
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    #endregion Soft Deletion

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Counts
    public int ItemCount { get; set; }
    #endregion Counts

    #region Constructor
    internal Order() { }
    #endregion Constructor
}