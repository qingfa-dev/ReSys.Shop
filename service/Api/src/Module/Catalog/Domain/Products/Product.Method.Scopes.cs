namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Scope Queries
    public static bool IsDraft(this Product product)
    {
        return product.Status == ProductStatus.Draft;
    }

    public static bool IsActive(this Product product)
    {
        return product.Status == ProductStatus.Active;
    }

    public static bool IsArchived(this Product product)
    {
        return product.Status == ProductStatus.Archived;
    }

    public static bool IsOnSale(this Product product, string? currency = null)
    {
        return product.Variants.Any(v =>
        {
            if (v.Prices is not { Count: > 0 })
            {
                return false;
            }

            return v.Prices.Any(p =>
                (currency == null || p.Currency == currency)
                && p.CompareAtAmount.HasValue
                && p.Amount.HasValue
                && p.CompareAtAmount > p.Amount);
        });
    }

    public static bool IsPurchasable(this Product product)
    {
        return product.Variants.Count == 0
            ? false
            : product.Variants.Any(v => !v.IsDeleted);
    }

    public static bool IsBackorderable(this Product product)
    {
        return product.Variants.Any(v => v.TrackInventory == false);
    }

    public static bool IsInStock(this Product product)
    {
        return product.Variants.Any(v => !v.IsDeleted);
    }

    public static ProductStatus ResolveStatus(this Product product)
    {
        if (product.IsDeleted)
        {
            return ProductStatus.Archived;
        }

        return product.Status;
    }
    #endregion
}
