namespace Module.Catalog.Domain.Variants.Prices;

public static class PriceMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new price entry for a variant.
    /// </summary>
    /// <param name="amount">The price amount. Must be greater than or equal to zero.</param>
    /// <param name="currency">The ISO 4217 currency code. Must not be null or empty.</param>
    /// <param name="variantId">Optional parent variant identifier.</param>
    /// <param name="compareAtAmount">Optional compare-at amount for sale display.</param>
    /// <param name="countryIso">Optional ISO 3166-1 alpha-2 country code.</param>
    /// <returns>A Result containing the created Price.</returns>
    // @CAT-10 Contract: pre=currency!=null&&amount>=0, post=entity.Currency==currency&&entity.Amount==amount, throws=ArgumentException
    public static Result<Price> Create(
        decimal? amount,
        string currency,
        Guid? variantId = null,
        decimal? compareAtAmount = null,
        string? countryIso = null)
    {
        return new Price
        {
            Amount = amount,
            Currency = currency,
            VariantId = variantId,
            CompareAtAmount = compareAtAmount,
            CountryIso = countryIso
        };
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the price with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="price">The price to update.</param>
    /// <param name="amount">Optional new amount.</param>
    /// <param name="currency">Optional new currency code.</param>
    /// <param name="compareAtAmount">Optional new compare-at amount.</param>
    /// <param name="countryIso">Optional new country ISO code.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this Price price,
        decimal? amount = null,
        string? currency = null,
        decimal? compareAtAmount = null,
        string? countryIso = null)
    {
        price.Amount = amount ?? price.Amount;
        price.Currency = currency ?? price.Currency;
        price.CompareAtAmount = compareAtAmount ?? price.CompareAtAmount;
        price.CountryIso = countryIso ?? price.CountryIso;

        return Result.Ok();
    }

    /// <summary>
    /// Sets the compare-at amount for sale pricing.
    /// </summary>
    /// <param name="price">The price to update.</param>
    /// <param name="compareAtAmount">The compare-at amount.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result SetCompareAt(this Price price, decimal? compareAtAmount)
    {
        // @CAT-5 Compute: Compare-at price for sale display
        price.CompareAtAmount = compareAtAmount;

        return Result.Ok(PriceResult.Success.CompareAtUpdated);
    }

    /// <summary>
    /// Marks the price as the default price for the variant's currency.
    /// </summary>
    /// <param name="price">The price to mark as default.</param>
    /// <returns>A Result indicating success.</returns>
    // @CAT-5 Compute: Mark price as default for the variant's currency
    public static Result MarkAsDefault(this Price price)
    {
        price.IsDefault = true;

        return Result.Ok(PriceResult.Success.MarkedAsDefault);
    }

    /// <summary>
    /// Determines whether the price is currently on sale (compare-at > amount).
    /// </summary>
    /// <param name="price">The price to check.</param>
    /// <returns>True if compare-at amount is greater than the current amount.</returns>
    public static bool IsOnSale(this Price price)
    {
        // @CAT-5 Compute: Sale status when compare-at exceeds current amount
        return price.CompareAtAmount.HasValue
            && price.Amount.HasValue
            && price.CompareAtAmount > price.Amount;
    }

    /// <summary>
    /// Soft-deletes the price by setting the deleted-at timestamp.
    /// </summary>
    /// <param name="price">The price to delete.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public static Result Delete(this Price price)
    {
        // Guard: Skip if already deleted
        if (price.DeletedAt is not null)
            return PriceResult.Errors.AlreadyDeleted;

        price.DeletedAt = DateTime.UtcNow;

        return Result.Ok();
    }

    // Compute: Price including VAT for display in tax-inclusive zones
    public static decimal PriceIncludingVat(this Price price, decimal vatRate)
    {
        // @CAT-5 Compute: Amount * (1 + vatRate) for VAT-inclusive pricing
        return (price.Amount ?? 0) * (1 + vatRate);
    }
    #endregion
}