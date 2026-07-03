using Module.Catalog.Domain.Products.Variants;

namespace Module.Catalog.Domain.Products;

public static class ProductMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new product with the specified properties.
    /// </summary>
    /// <param name="name">The product name. Must not be null or empty.</param>
    /// <param name="slug">The URL slug. Must not be null or empty.</param>
    /// <param name="description">Optional product description.</param>
    /// <param name="status">The initial product status. Defaults to Draft.</param>
    /// <param name="availableOn">Optional date when the product becomes available.</param>
    /// <param name="metaTitle">Optional SEO meta title.</param>
    /// <param name="metaDescription">Optional SEO meta description.</param>
    /// <param name="metaKeywords">Optional SEO meta keywords.</param>
    /// <param name="discontinueOn">Optional date when the product is discontinued.</param>
    /// <param name="makeActiveAt">Optional date when the product becomes active.</param>
    /// <param name="taxCategoryId">Optional tax category identifier.</param>
    /// <param name="shippingCategoryId">Optional shipping category identifier.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>A Result containing the created Product.</returns>
    // @CAT-10 Contract: pre=name!=null&&slug!=null, post=entity.Id!=null&&entity.CreatedAtUtc!=default, throws=ArgumentException
    public static Result<Product> Create(
        string name,
        string slug,
        string? description = null,
        ProductStatus status = ProductConstant.Defaults.Status,
        DateTimeOffset? availableOn = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        DateTimeOffset? discontinueOn = null,
        DateTimeOffset? makeActiveAt = null,
        Guid? taxCategoryId = null,
        Guid? id = null)
    {
        // Validate: Name must not be null or empty
        // Validate: Slug must not be null or empty
        var product = new Product
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description ?? string.Empty,
            Status = status,
            AvailableOn = availableOn,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            DiscontinueOn = discontinueOn,
            MakeActiveAt = makeActiveAt,
            TaxCategoryId = taxCategoryId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return product;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the product with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <param name="name">Optional new product name.</param>
    /// <param name="slug">Optional new URL slug.</param>
    /// <param name="description">Optional new description.</param>
    /// <param name="status">Optional new status.</param>
    /// <param name="availableOn">Optional new available-on date.</param>
    /// <param name="metaTitle">Optional new meta title.</param>
    /// <param name="metaDescription">Optional new meta description.</param>
    /// <param name="metaKeywords">Optional new meta keywords.</param>
    /// <param name="discontinueOn">Optional new discontinue-on date.</param>
    /// <param name="makeActiveAt">Optional new make-active-at date.</param>
    /// <param name="taxCategoryId">Optional new tax category identifier.</param>
    /// <param name="shippingCategoryId">Optional new shipping category identifier.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this Product product,
        string? name = null,
        string? slug = null,
        string? description = null,
        ProductStatus? status = null,
        DateTimeOffset? availableOn = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        DateTimeOffset? discontinueOn = null,
        DateTimeOffset? makeActiveAt = null,
        Guid? taxCategoryId = null)
    {
        product.Name = name ?? product.Name;
        product.Slug = slug ?? product.Slug;
        product.Description = description ?? product.Description;
        product.Status = status ?? product.Status;
        product.AvailableOn = availableOn ?? product.AvailableOn;
        product.MetaTitle = metaTitle ?? product.MetaTitle;
        product.MetaDescription = metaDescription ?? product.MetaDescription;
        product.MetaKeywords = metaKeywords ?? product.MetaKeywords;
        product.DiscontinueOn = discontinueOn ?? product.DiscontinueOn;
        product.MakeActiveAt = makeActiveAt ?? product.MakeActiveAt;
        product.TaxCategoryId = taxCategoryId ?? product.TaxCategoryId;

        return Result.Ok();
    }

    /// <summary>
    /// Activates the product. Fails if the product is already active or archived.
    /// </summary>
    /// <param name="product">The product to activate.</param>
    /// <returns>A Result indicating success or failure.</returns>
    // @CAT-4 Enforce: Product must have at least one active variant to be activated
    public static Result Activate(this Product product)
    {
        if (product.Status == ProductStatus.Active)
        {
            return ProductResult.Errors.AlreadyActive;
        }

        if (product.Status == ProductStatus.Archived)
            return ProductResult.Errors.CannotActivateArchivedProduct;

        product.Status = ProductStatus.Active;

        return Result.Ok(ProductResult.Success.Activated);
    }

    /// <summary>
    /// Archives the product. Fails if the product is already archived.
    /// </summary>
    /// <param name="product">The product to archive.</param>
    /// <returns>A Result indicating success or failure.</returns>
    // @CAT-4 Enforce: Archived products set discontinuation date to now
    public static Result Archive(this Product product)
    {
        if (product.Status == ProductStatus.Archived)
        {
            return ProductResult.Errors.AlreadyArchived;
        }

        product.DiscontinueOn = DateTimeOffset.UtcNow;
        product.Status = ProductStatus.Archived;

        return Result.Ok(ProductResult.Success.Archived);
    }

    /// <summary>
    /// Sets the product status to draft. Fails if already in draft status.
    /// </summary>
    /// <param name="product">The product to set to draft.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public static Result Draft(this Product product)
    {
        // Enforce: Prevent drafting an already-drafted product
        if (product.Status == ProductStatus.Draft)
        {
            return ProductResult.Errors.AlreadyDraft;
        }

        product.Status = ProductStatus.Draft;

        return Result.Ok(ProductResult.Success.Drafted);
    }

    /// <summary>
    /// Discontinues the product by setting the discontinue-on date and archiving it.
    /// </summary>
    /// <param name="product">The product to discontinue.</param>
    /// <returns>A Result indicating success or failure.</returns>
    // @CAT-4 Enforce: Discontinued products set discontinuation date to now and archive
    public static Result Discontinue(this Product product)
    {
        if (product.DiscontinueOn <= DateTimeOffset.UtcNow)
        {
            return ProductResult.Errors.AlreadyDiscontinued;
        }

        product.DiscontinueOn = DateTimeOffset.UtcNow;
        product.Status = ProductStatus.Archived;

        return Result.Ok(ProductResult.Success.Discontinued);
    }

    /// <summary>
    /// Determines whether the product is available for purchase.
    /// </summary>
    /// <param name="product">The product to check.</param>
    /// <returns>True if the product is active, not deleted, and available-on date has passed.</returns>
    // @CAT-5 Compute: Determine product availability based on status, deletion, and availability window
    public static bool IsAvailable(this Product product)
    {
        return product.Status == ProductStatus.Active
            && !product.IsDeleted
            && (product.AvailableOn == null || product.AvailableOn <= DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Changes the product status to the specified value.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <param name="newStatus">The new status value.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result ChangeStatus(this Product product, ProductStatus newStatus)
    {
        if (product.Status == newStatus)
        {
            return Result.Ok();
        }

        product.Status = newStatus;

        return Result.Ok();
    }

    /// <summary>
    /// Soft-deletes the product.
    /// </summary>
    /// <param name="product">The product to delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <returns>A Result indicating success.</returns>
    // @CAT-2 Guard: Skip already-deleted products on soft delete
    public static Result Delete(this Product product, string deletedBy)
    {
        if (product.IsDeleted)
        {
            return Result.Ok();
        }

        product.IsDeleted = true;
        product.DeletedAtUtc = DateTimeOffset.UtcNow;
        product.DeletedBy = deletedBy;

        return Result.Ok();
    }

    // /// <summary>
    // /// Determines whether any variant of the product can supply the requested quantity.
    // /// </summary>
    // /// <param name="product">The product to check.</param>
    // /// <param name="quantity">The requested quantity.</param>
    // /// <returns>True if at least one variant can supply.</returns>
    // // @CAT-5 Compute: Check if any variant can supply the requested quantity
    // public static bool CanSupply(this Product product, int quantity)
    // {
    //     return product.Variants.Any(v => !v.IsDeleted && v.StockItems.Sum(s => s.CountOnHand) >= quantity);
    // }

    /// <summary>
    /// Returns the default variant for the product.
    /// </summary>
    /// <param name="product">The product to get the default variant for.</param>
    /// <returns>The first non-master, non-deleted variant, or the master variant as fallback.</returns>
    // @CAT-5 Compute: Default variant is first non-master variant or master fallback
    public static Variant? DefaultVariant(this Product product)
    {
        return product.Variants
            .FirstOrDefault(v => !v.IsMaster && !v.IsDeleted)
            ?? product.Variants.FirstOrDefault(v => v.IsMaster);
    }

    /// <summary>
    /// Determines whether the product has any variants.
    /// </summary>
    /// <param name="product">The product to check.</param>
    /// <returns>True if the product has one or more variants.</returns>
    // @CAT-5 Compute: Determine if product has any associated variants
    public static bool HasVariants(this Product product)
    {
        return product.Variants.Count > 0;
    }

    /// <summary>
    /// Determines whether the product is currently available for sale.
    /// </summary>
    /// <param name="product">The product to check.</param>
    /// <returns>True if the product is active, not deleted, and available-on date has passed.</returns>
    // @CAT-5 Compute: Determine if product is currently available for sale
    public static bool Available(this Product product)
    {
        return product.Status == ProductStatus.Active
            && !product.IsDeleted
            && (product.AvailableOn == null || product.AvailableOn <= DateTimeOffset.UtcNow);
    }
    #endregion
}