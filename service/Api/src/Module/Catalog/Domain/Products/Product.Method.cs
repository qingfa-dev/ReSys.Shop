using Module.Catalog.Domain.Products.Variants;

namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Factory Methods
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
