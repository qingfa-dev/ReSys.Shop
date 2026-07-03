namespace Module.Catalog.Domain.Products.Variants.Prices;

public static class PriceHistoryMethod
{
    #region Factory Methods
    // Contract: pre=amount>=0&&currency!=null, post=entity.Amount==amount&&entity.Currency==currency
    public static Result<PriceHistory> Create(
        decimal amount,
        string currency,
        Guid priceId,
        Guid variantId,
        DateTimeOffset? recordedAt = null)
    {
        var history = new PriceHistory
        {
            Amount = amount,
            Currency = currency,
            PriceId = priceId,
            VariantId = variantId,
            RecordedAt = recordedAt ?? DateTimeOffset.UtcNow,
        };

        return history;
    }
    #endregion

    #region Methods
    // Format: Display the price amount with currency code
    public static string DisplayAmount(this PriceHistory history)
    {
        return $"{history.Amount:N2} {history.Currency}";
    }

    // Compute: Convert the amount to cents for precision arithmetic
    public static long AmountInCents(this PriceHistory history)
    {
        return (long)(history.Amount * 100);
    }
    #endregion
}
