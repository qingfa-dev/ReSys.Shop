using Shared.Application.Domain.Concerns.Auditable;

using Slugify;

namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Factory Methods
    public static Result<Product> Create(
    #region Properties
        string name,
        string? description = null,
        ProductStatus status = ProductStatus.Draft,
    #endregion Properties
    #region SEO
        string? slug = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
    #endregion SEO
    #region Timestamp
        DateTimeOffset? availableOn = null,
        DateTimeOffset? discontinueOn = null,
        DateTimeOffset? makeActiveAt = null,
    #endregion Timestamp
    #region Fashion
        string? styleCode = null,
        string? seasonName = null,
        string? materialComposition = null,
        string? careInstructions = null,
        string? fitNotes = null,
        string? department = null,
        string? genderTarget = null,
    #endregion Fashion
        Guid? id = null)
    {
        var product = new Product
        {
            Id = id ?? Guid.NewGuid(),
            #region Properties
            Name = name,
            Description = description ?? string.Empty,
            Status = status,
            #endregion Properties

            #region SEO
            Slug = slug ?? new SlugHelper().GenerateSlug(name),
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            #endregion SEO

            #region Timestamp
            AvailableOn = availableOn,
            DiscontinueOn = discontinueOn,
            MakeActiveAt = makeActiveAt,
            #endregion Timestamp

            #region Fashion
            StyleCode = styleCode,
            SeasonName = seasonName,
            MaterialComposition = materialComposition,
            CareInstructions = careInstructions,
            FitNotes = fitNotes,
            Department = department,
            GenderTarget = genderTarget,
            #endregion Fashion
        };

        AuditableBehavior.Create(product);
        return product;
    }
    #endregion

    #region Lifecycle Methods
    public static Result Update(this Product product,
    #region Properties
        string? name,
        string? description = null,
        ProductStatus? status = ProductStatus.Draft,
    #endregion Properties
    #region SEO
        string? slug = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
    #endregion SEO
    #region Timestamp
        DateTimeOffset? availableOn = null,
        DateTimeOffset? discontinueOn = null,
        DateTimeOffset? makeActiveAt = null,
    #endregion Timestamp
    #region Fashion
        string? styleCode = null,
        string? seasonName = null,
        string? materialComposition = null,
        string? careInstructions = null,
        string? fitNotes = null,
        string? department = null,
        string? genderTarget = null
    #endregion Fashion
    )
    {
        #region Properties
        product.Name = name ?? product.Name;
        product.Description = description ?? product.Description;
        product.Status = status ?? product.Status;
        #endregion Properties

        #region SEO
        product.Slug = slug ?? product.Slug;
        product.MetaTitle = metaTitle ?? product.MetaTitle;
        product.MetaDescription = metaDescription ?? product.MetaDescription;
        product.MetaKeywords = metaKeywords ?? product.MetaKeywords;
        #endregion SEO

        #region Timestamp
        product.AvailableOn = availableOn ?? product.AvailableOn;
        product.DiscontinueOn = discontinueOn ?? product.DiscontinueOn;
        product.MakeActiveAt = makeActiveAt ?? product.MakeActiveAt;
        #endregion Timestamp

        #region Fashion
        product.StyleCode = styleCode ?? product.StyleCode;
        product.SeasonName = seasonName ?? product.SeasonName;
        product.MaterialComposition = materialComposition ?? product.MaterialComposition;
        product.CareInstructions = careInstructions ?? product.CareInstructions;
        product.FitNotes = fitNotes ?? product.FitNotes;
        product.Department = department ?? product.Department;
        product.GenderTarget = genderTarget ?? product.GenderTarget;
        #endregion Fashion

        AuditableBehavior.Touch(product);
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