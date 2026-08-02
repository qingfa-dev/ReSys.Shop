namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Factory Methods
    public static Result<Product> Create(
        string name,
        string slug,
        string? description = null,
        ProductStatus status = ProductStatus.Draft,
        DateTimeOffset? availableOn = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        DateTimeOffset? discontinueOn = null,
        DateTimeOffset? makeActiveAt = null,
        // Fashion
        string? styleCode = null,
        string? seasonName = null,
        string? materialComposition = null,
        string? careInstructions = null,
        string? fitNotes = null,
        string? department = null,
        string? genderTarget = null,
        Guid? id = null)
    {
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
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System",
            // Fashion
            StyleCode = styleCode,
            SeasonName = seasonName,
            MaterialComposition = materialComposition,
            CareInstructions = careInstructions,
            FitNotes = fitNotes,
            Department = department,
            GenderTarget = genderTarget
        };

        return product;
    }
    #endregion

    #region Lifecycle Methods
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
        // Fashion
        string? styleCode = null,
        string? seasonName = null,
        string? materialComposition = null,
        string? careInstructions = null,
        string? fitNotes = null,
        string? department = null,
        string? genderTarget = null)
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

        // Fashion
        product.StyleCode = styleCode ?? product.StyleCode;
        product.SeasonName = seasonName ?? product.SeasonName;
        product.MaterialComposition = materialComposition ?? product.MaterialComposition;
        product.CareInstructions = careInstructions ?? product.CareInstructions;
        product.FitNotes = fitNotes ?? product.FitNotes;
        product.Department = department ?? product.Department;
        product.GenderTarget = genderTarget ?? product.GenderTarget;

        return Result.Ok();
    }

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
    #endregion
}