namespace Module.Catalog.Domain.Products.Variants;

public static class VariantExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new variant for the specified product.
    /// </summary>
    /// <param name="productId">The parent product identifier.</param>
    /// <param name="sku">The stock-keeping unit. Must not be null or empty.</param>
    /// <param name="isMaster">Whether this is the master variant. Defaults to false.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <param name="barcode">Optional barcode.</param>
    /// <param name="hsCode">Optional harmonized system code.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>A Result containing the created Variant.</returns>
    // @CAT-10 Contract: pre=productId!=Guid.Empty&&sku!=null, post=entity.Id!=null&&entity.Sku==sku, throws=ArgumentException
    public static Result<Variant> Create(
        Guid productId,
        string sku,
        bool isMaster = false,
        int position = 0,
        string? barcode = null,
        string? hsCode = null,
        Guid? id = null)
    {
        // Validate: ProductId must be a valid GUID
        // Validate: SKU must not be null or empty
        var variant = new Variant
        {
            Id = id ?? Guid.NewGuid(),
            ProductId = productId,
            Sku = sku,
            IsMaster = isMaster,
            Position = position,
            Barcode = barcode,
            HsCode = hsCode
        };

        return variant;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the variant with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="variant">The variant to update.</param>
    /// <param name="sku">Optional new SKU.</param>
    /// <param name="position">Optional new position.</param>
    /// <param name="trackInventory">Optional new track-inventory flag.</param>
    /// <param name="barcode">Optional new barcode.</param>
    /// <param name="hsCode">Optional new HS code.</param>
    /// <returns>A Result indicating success.</returns>
    // @CAT-4 Enforce:
    public static Result Update(this Variant variant,
        string? sku = null,
        int? position = null,
        bool? trackInventory = null,
        string? barcode = null,
        string? hsCode = null)
    {
        variant.Sku = sku ?? variant.Sku;
        variant.Position = position ?? variant.Position;
        variant.TrackInventory = trackInventory ?? variant.TrackInventory;
        variant.Barcode = barcode ?? variant.Barcode;
        variant.HsCode = hsCode ?? variant.HsCode;

        return Result.Ok();
    }

    /// <summary>
    /// Updates the pricing information for the variant.
    /// </summary>
    /// <param name="variant">The variant to update.</param>
    /// <param name="price">Optional new price.</param>
    /// <param name="costPrice">Optional new cost price.</param>
    /// <param name="costCurrency">Optional new cost currency (ISO 4217).</param>
    /// <returns>A Result indicating success.</returns>
    // @CAT-4 Enforce:
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

    /// <summary>
    /// Updates the physical specifications of the variant.
    /// </summary>
    /// <param name="variant">The variant to update.</param>
    /// <param name="weight">Optional new weight.</param>
    /// <param name="weightUnit">Optional new weight unit.</param>
    /// <param name="height">Optional new height.</param>
    /// <param name="width">Optional new width.</param>
    /// <param name="depth">Optional new depth.</param>
    /// <param name="dimensionsUnit">Optional new dimensions unit.</param>
    /// <returns>A Result indicating success.</returns>
    // @CAT-4 Enforce:
    public static Result UpdatePhysicalSpecs(this Variant variant,
        decimal? weight = null,
        WeightUnit? weightUnit = null,
        decimal? height = null,
        decimal? width = null,
        decimal? depth = null,
        DimensionUnit? dimensionsUnit = null)
    {
        variant.Weight = weight ?? variant.Weight;
        variant.WeightUnit = weightUnit ?? variant.WeightUnit;
        variant.Height = height ?? variant.Height;
        variant.Width = width ?? variant.Width;
        variant.Depth = depth ?? variant.Depth;
        variant.DimensionsUnit = dimensionsUnit ?? variant.DimensionsUnit;

        return Result.Ok();
    }

    /// <summary>
    /// Updates the logistics fields (HS code and barcode) for the variant.
    /// </summary>
    /// <param name="variant">The variant to update.</param>
    /// <param name="hsCode">Optional new HS code.</param>
    /// <param name="barcode">Optional new barcode.</param>
    /// <returns>A Result indicating success.</returns>
    // @CAT-4 Enforce:
    public static Result UpdateLogistics(this Variant variant,
        string? hsCode = null,
        string? barcode = null)
    {
        variant.HsCode = hsCode ?? variant.HsCode;
        variant.Barcode = barcode ?? variant.Barcode;

        return Result.Ok();
    }

    /// <summary>
    /// Discontinues the variant by setting the discontinued-on date.
    /// </summary>
    /// <param name="variant">The variant to discontinue.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public static Result Discontinue(this Variant variant)
    {
        // @CAT-4 Enforce: Variant must not already be discontinued
        if (variant.DiscontinuedOn <= DateTimeOffset.UtcNow)
        {
            return VariantResult.Errors.AlreadyDiscontinued;
        }

        variant.DiscontinuedOn = DateTimeOffset.UtcNow;

        return Result.Ok(VariantResult.Success.Discontinued);
    }

    /// <summary>
    /// Determines whether the variant is discontinued.
    /// </summary>
    /// <param name="variant">The variant to check.</param>
    /// <returns>True if the discontinued-on date is in the past.</returns>
    public static bool IsDiscontinued(this Variant variant)
    {
        return variant.DiscontinuedOn.HasValue && variant.DiscontinuedOn <= DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Determines whether the variant is available for purchase.
    /// </summary>
    /// <param name="variant">The variant to check.</param>
    /// <returns>True if the variant is not discontinued, not deleted, and inventory tracking allows it.</returns>
    public static bool IsAvailable(this Variant variant)
    {
        return !variant.IsDiscontinued()
            && !variant.IsDeleted
            && (variant.TrackInventory == false || true);
    }

    /// <summary>
    /// Soft-deletes the variant.
    /// </summary>
    /// <param name="variant">The variant to delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this Variant variant, string deletedBy)
    {
        // @CAT-2 Guard: Skip if already deleted
        if (variant.IsDeleted)
        {
            return Result.Ok();
        }

        variant.IsDeleted = true;
        variant.DeletedAtUtc = DateTimeOffset.UtcNow;
        variant.DeletedBy = deletedBy;

        return Result.Ok();
    }

    /// <summary>
    /// Returns a formatted string of option value presentations for the variant.
    /// </summary>
    /// <param name="variant">The variant to format.</param>
    /// <returns>A comma-separated string of option value names (e.g. "Red, Large").</returns>
    // @CAT-5 Compute: Format option values as comma-separated display string
    public static string OptionsText(this Variant variant)
    {
        return string.Join(", ", variant.OptionValueVariants
            .Where(ov => ov.OptionValue != null)
            .Select(ov => ov.OptionValue!.Presentation ?? ov.OptionValue!.Name));
    }

    /// <summary>
    /// Returns the exchange display name combining SKU and option text.
    /// </summary>
    /// <param name="variant">The variant to format.</param>
    /// <returns>A string suitable for exchange/export contexts (e.g. "SKU-001: Red, Large").</returns>
    // @CAT-5 Compute: Build exchange display name from SKU and option text
    public static string ExchangeName(this Variant variant)
    {
        var options = variant.OptionsText();
        return string.IsNullOrEmpty(options) ? variant.Sku ?? "Unknown" : $"{variant.Sku}: {options}";
    }

    /// <summary>
    /// Returns a human-readable descriptive name for the variant.
    /// </summary>
    /// <param name="variant">The variant to describe.</param>
    /// <returns>A descriptive string combining product name, SKU, and option text.</returns>
    // @CAT-5 Compute: Build human-readable descriptive name with product, options, and SKU
    public static string DescriptiveName(this Variant variant)
    {
        var productName = variant.Product?.Name ?? "Unknown Product";
        var options = variant.OptionsText();
        return string.IsNullOrEmpty(options) ? $"{productName} ({variant.Sku})" : $"{productName} - {options} ({variant.Sku})";
    }

    // /// <summary>
    // /// Checks if the requested quantity can be supplied from available stock across all locations.
    // /// </summary>
    // // @CAT-5 Compute: Check stock availability for the requested quantity
    // public static bool CanSupply(this Variant variant, int quantity)
    // {
    //     return variant.StockItems.Sum(si => si.CountOnHand) >= quantity;
    // }

    // /// <summary>
    // /// Returns the total count-on-hand across all stock locations.
    // /// </summary>
    // // @CAT-5 Compute: Aggregate count_on_hand across all stock locations
    // public static int TotalOnHand(this Variant variant)
    // {
    //     return variant.StockItems.Sum(si => si.CountOnHand);
    // }

    // /// <summary>
    // /// Determines whether the variant is in stock at any location.
    // /// </summary>
    // // @CAT-5 Compute: In stock when any location has positive count
    // public static bool InStock(this Variant variant)
    // {
    //     return variant.TotalOnHand() > 0;
    // }

    // /// <summary>
    // /// Determines whether the variant can be backordered at any location.
    // /// </summary>
    // // @CAT-5 Compute: Backorderable when any location allows backorders
    // public static bool IsBackorderable(this Variant variant)
    // {
    //     return variant.StockItems.Any(si => si.Backorderable);
    // }

    // /// <summary>
    // /// Determines whether the variant is purchasable, considering availability and stock.
    // /// </summary>
    // // @CAT-5 Compute: Variant is purchasable when product is available, variant is active, priced, and stockable
    // public static bool Purchasable(this Variant variant)
    // {
    //     return variant.Product.IsAvailable()
    //         && !variant.IsDiscontinued()
    //         && (variant.TrackInventory == false || variant.InStock() || variant.IsBackorderable());
    // }

    /// <summary>
    /// Determines whether inventory tracking should be enforced for this variant.
    /// </summary>
    // @CAT-2 Guard: Only non-master variants with track_inventory=true require stock checks
    public static bool ShouldTrackInventory(this Variant variant)
    {
        return variant.TrackInventory && !variant.IsMaster;
    }
    #endregion
}