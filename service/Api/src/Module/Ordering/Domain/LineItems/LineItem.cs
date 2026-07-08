using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Domain.LineItems;

/// <summary>
/// Represents a single line item within an order, tracking quantity, pricing, and adjustments.
/// </summary>
// @CAT-10 Invariant: Quantity >= 1; Total = (Quantity * Price) + AdjustmentTotal; CostPrice <= Price when set
public sealed partial class LineItem : Entity, IAuditable
{
    #region Properties
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public decimal AdjustmentTotal { get; set; }
    public string Currency { get; set; } = "USD";
    #endregion Properties

    #region Relationships
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid VariantId { get; set; }
    public Catalog.Domain.Products.Variants.Variant Variant { get; set; } = null!;
    #endregion Relationships

    #region Auditing
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal LineItem() { } // For EF Core
    #endregion Constructor
}
