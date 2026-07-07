using Shared.Application.Domain.Models;

namespace Module.Promotions.Domain.OrderPromotions;

/// <summary>Represents the application of a promotion to an order, with optional coupon code tracking.</summary>
// Invariant: OrderId and PromotionId must reference valid entities; PromotionCodeId is optional for multi-code promotions
public sealed partial class OrderPromotion : Entity
{
    #region Properties
    public Guid OrderId { get; set; }
    public Guid PromotionId { get; set; }
    public Guid? PromotionCodeId { get; set; }
    #endregion Properties

    #region Constructor
    internal OrderPromotion() { }
    #endregion Constructor
}
