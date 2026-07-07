using System.Globalization;

namespace Module.Shipping.Domain.ShippingRates;

public static class ShippingRateExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new shipping rate for a shipping method.
    /// </summary>
    /// <param name="name">The rate display name. Must not be empty.</param>
    /// <param name="cost">The base cost. Must be greater than zero.</param>
    /// <param name="shippingMethodId">The shipping method identifier.</param>
    /// <param name="deliveryRange">Optional delivery range description.</param>
    /// <returns>A result containing the newly created shipping rate.</returns>
    // @CAT-10 Contract: pre=cost>0 && name!=null, post=rate.Selected==false && rate.FinalPrice==cost, throws=none
    public static Result<ShippingRate> Create(
        string name,
        decimal cost,
        Guid shippingMethodId,
        string? deliveryRange = null,
        decimal? minWeight = null,
        decimal? maxWeight = null,
        decimal? freeShippingThreshold = null)
    {
        if (cost <= 0)
        {
            return ShippingRateResult.Errors.CostRequired;
        }

        // Validate: MinWeight must be <= MaxWeight when both set
        if (minWeight.HasValue && maxWeight.HasValue && minWeight.Value > maxWeight.Value)
            return ShippingRateResult.Errors.MinWeightExceedsMaxWeight;

        // Validate: Weights must be non-negative
        if (minWeight.HasValue && minWeight.Value < 0) return ShippingRateResult.Errors.WeightNegative;
        if (maxWeight.HasValue && maxWeight.Value < 0) return ShippingRateResult.Errors.WeightNegative;

        var rate = new ShippingRate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Cost = cost,
            // Compute: FinalPrice starts at cost; adjusted by discounts and promotions later
            FinalPrice = cost,
            Selected = ShippingRateConstant.Defaults.Selected,
            DisplayPrice = cost.ToString("F2", CultureInfo.InvariantCulture),
            DeliveryRange = deliveryRange,
            MinWeight = minWeight,
            MaxWeight = maxWeight,
            FreeShippingThreshold = freeShippingThreshold,
            ShippingMethodId = shippingMethodId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return rate;
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>
    /// Selects the shipping rate as the chosen rate for the shipment.
    /// </summary>
    /// <param name="rate">The shipping rate to select.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-4 Enforce: Rate must not already be selected
    public static Result Select(this ShippingRate rate)
    {
        if (rate.Selected)
        {
            return ShippingRateResult.Errors.AlreadySelected;
        }

        rate.Selected = true;
        rate.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShippingRateResult.Success.Selected(rate.Id));
    }

    /// <summary>
    /// Unselects the shipping rate, making it no longer the chosen rate.
    /// </summary>
    /// <param name="rate">The shipping rate to unselect.</param>
    /// <returns>A result indicating success or failure.</returns>
    // Enforce: Rate must currently be selected
    public static Result Unselect(this ShippingRate rate)
    {
        if (!rate.Selected)
        {
            return ShippingRateResult.Errors.NotSelected;
        }

        rate.Selected = false;
        rate.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShippingRateResult.Success.Unselected(rate.Id));
    }

    /// <summary>
    /// Determines whether the shipping rate is free (cost zero or less).
    /// </summary>
    /// <param name="rate">The shipping rate to check.</param>
    /// <returns>True if the rate is free; otherwise false.</returns>
    // Compute: Free when cost is zero or negative (Ruby SDK shipping_rate.rb#free? alignment)
    public static bool IsFree(this ShippingRate rate)
    {
        return rate.Cost <= 0m;
    }
    #endregion Methods
}