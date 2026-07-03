namespace Module.Catalog.Domain.Products;

public static class ProductScopesExtensions
{
    // Check: Determine if the product is in draft status
    public static bool IsDraft(this Product product)
    {
        return product.Status == ProductStatus.Draft;
    }

    // Check: Determine if the product is active
    public static bool IsActive(this Product product)
    {
        return product.Status == ProductStatus.Active;
    }

    // Check: Determine if the product is archived
    public static bool IsArchived(this Product product)
    {
        return product.Status == ProductStatus.Archived;
    }

    // Compute: Determine if any variant has a compare-at price indicating a sale
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

    // Compute: Determine if the product has any purchasable variants
    public static bool IsPurchasable(this Product product)
    {
        return product.Variants.Count == 0
            ? false
            : product.Variants.Any(v => !v.IsDeleted);
    }

    // Check: Determine if any variant has inventory tracking disabled
    public static bool IsBackorderable(this Product product)
    {
        return product.Variants.Any(v => v.TrackInventory == false);
    }

    // Check: Determine if any variant is available (not deleted)
    public static bool IsInStock(this Product product)
    {
        return product.Variants.Any(v => !v.IsDeleted);
    }

    // Compute: Resolve the effective product status considering soft-deletion
    public static ProductStatus ResolveStatus(this Product product)
    {
        if (product.IsDeleted)
        {
            return ProductStatus.Archived;
        }

        return product.Status;
    }
}
