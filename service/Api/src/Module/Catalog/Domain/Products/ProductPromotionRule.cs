using Shared.Application.Domain.Models;

namespace Module.Catalog.Domain.Products;

/// <summary>
/// Represents the association between a product and a promotion rule.
/// </summary>
// Invariant: ProductId != Guid.Empty; PromotionRuleId != Guid.Empty
public sealed partial class ProductPromotionRule : Entity
{
    #region Properties
    public Guid ProductId { get; set; }
    public Guid PromotionRuleId { get; set; }
    #endregion
}
