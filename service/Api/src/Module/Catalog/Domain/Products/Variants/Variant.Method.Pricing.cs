namespace Module.Catalog.Domain.Products.Variants;

public static partial class VariantMethod
{
    #region Pricing Methods
    public static Result UpdatePricing(this Variant variant,
        decimal? price = null,
        decimal? costPrice = null,
        string? costCurrency = null)
    {
        variant.Price = price ?? variant.Price;
        variant.CostPrice = costPrice ?? variant.CostPrice;
        variant.CostCurrency = costCurrency ?? variant.CostCurrency;

        return Result.Ok();
    }
    #endregion
}
