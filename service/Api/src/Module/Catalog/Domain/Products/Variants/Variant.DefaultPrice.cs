namespace Module.Catalog.Domain.Products.Variants;

public static class VariantDefaultPriceExtensions
{
    // Contract: pre=productId!=Guid.Empty&&sku!=null, post=entity!=null&&entity.Price==defaultPrice
    public static Result<Variant> CreateWithDefaultPrice(
        Guid productId,
        string sku,
        decimal? defaultPrice,
        string currency = "USD",
        bool isMaster = false,
        int position = 0,
        string? barcode = null,
        string? hsCode = null,
        Guid? id = null)
    {
        var variant = new Variant
        {
            Id = id ?? Guid.NewGuid(),
            ProductId = productId,
            Sku = sku,
            IsMaster = isMaster,
            Position = position,
            Barcode = barcode,
            HsCode = hsCode,
            Price = defaultPrice,
            CostCurrency = currency,
        };

        return variant;
    }

    // Filter: Select base prices that are not associated with a price list
    public static decimal? DefaultPriceForCurrency(this Variant variant, string currency)
    {
        if (variant.Prices is { Count: > 0 })
        {
            return variant.Prices
                .Where(p => p.Currency == currency && p.PriceListId == null)
                .Select(p => p.Amount)
                .FirstOrDefault();
        }

        return default;
    }

    // Assign: Set the default price and currency on the variant
    public static Result SetDefaultPrice(this Variant variant, decimal? amount, string currency)
    {
        variant.Price = amount;
        variant.CostCurrency = currency;
        return Result.Ok();
    }
}
