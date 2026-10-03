namespace Module.Customer.Domain.Wishlists.WishedItems;

public static class WishedItemMethod
{
    #region Factory Methods

    public static Result<WishedItem> Create(
        Guid variantId,
        Guid wishlistId,
        int quantity = WishedItemConstant.Defaults.Quantity)
    {
        if (variantId == Guid.Empty)
            return WishedItemResult.Failure.VariantIdRequired;

        if (wishlistId == Guid.Empty)
            return WishedItemResult.Failure.WishlistIdRequired;

        if (quantity < WishedItemConstant.Constraints.MinQuantity)
            return WishedItemResult.Failure.QuantityTooLow;

        if (quantity > WishedItemConstant.Constraints.MaxQuantity)
            return WishedItemResult.Failure.QuantityTooHigh;

        return new WishedItem { VariantId = variantId, WishlistId = wishlistId, Quantity = quantity };
    }

    #endregion

    #region Update

    public static Result<WishedItem> Update(
        this WishedItem item,
        int? quantity = default)
    {
        if (quantity.HasValue)
        {
            if (quantity.Value < WishedItemConstant.Constraints.MinQuantity)
                return WishedItemResult.Failure.QuantityTooLow;
            if (quantity.Value > WishedItemConstant.Constraints.MaxQuantity)
                return WishedItemResult.Failure.QuantityTooHigh;
            item.Quantity = quantity.Value;
        }

        return item;
    }

    #endregion
}