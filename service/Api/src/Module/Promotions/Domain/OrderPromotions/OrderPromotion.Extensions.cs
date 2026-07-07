namespace Module.Promotions.Domain.OrderPromotions;

public static class OrderPromotionExtensions
{
    #region Factory Methods
    /// <summary>Creates a link between an order and a promotion for tracking applied promotions.</summary>
    /// <param name="orderId">The identifier of the order.</param>
    /// <param name="promotionId">The identifier of the applied promotion.</param>
    /// <param name="promotionCodeId">Optional identifier of the specific coupon code used.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A Result containing the created OrderPromotion on success.</returns>
    // Contract: pre=orderId is not default, promotionId is not default, post=entity.Id is not default, throws=none
    public static Result<OrderPromotion> Create(
        Guid orderId,
        Guid promotionId,
        Guid? promotionCodeId = null,
        Guid? id = null)
    {
        return new OrderPromotion
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId,
            PromotionId = promotionId,
            PromotionCodeId = promotionCodeId
        };
    }
    #endregion Factory Methods
}