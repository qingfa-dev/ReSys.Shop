using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Domain.LineItems;

/// <summary>
/// Represents a single line item within an order, tracking quantity, pricing, and adjustments.
/// </summary>
// @CAT-10 Invariant: Quantity >= 1; Total = (Quantity * Price) + AdjustmentTotal; CostPrice <= Price when set
// @CAT-10 Boundary: Domain → Persistence — EF Core entity; do not add persistence concerns to domain logic
public sealed partial class LineItem : Entity, IAuditable
{
    #region Properties
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public decimal AdjustmentTotal { get; set; }
    public string Currency { get; set; } = OrderConstant.Defaults.Currency;
    #endregion Properties

    #region Relationships
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid VariantId { get; set; }
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    // Boundary: Persistence → Domain — reserved for EF Core materialization, do not invoke from application code
    internal LineItem() { }
    #endregion Constructor
}
