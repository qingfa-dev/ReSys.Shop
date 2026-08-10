using Module.Catalog.Domain.Variants;

namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Availability Queries
    public static bool IsAvailable(this Product product)
    {
        return product.Status == ProductStatus.Active
            && !product.IsDeleted
            && (product.AvailableOn == null || product.AvailableOn <= DateTimeOffset.UtcNow);
    }

    public static Variant? DefaultVariant(this Product product)
    {
        return product.Variants
            .FirstOrDefault(v => !v.IsMaster && !v.IsDeleted)
            ?? product.Variants.FirstOrDefault(v => v.IsMaster);
    }

    public static bool HasVariants(this Product product)
    {
        return product.Variants.Count > 0;
    }
    #endregion
}