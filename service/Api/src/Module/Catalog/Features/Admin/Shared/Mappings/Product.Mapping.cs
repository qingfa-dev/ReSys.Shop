using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Mappings;

/// <summary>
/// Maps between ProductRequest DTOs and Product domain entities.
/// </summary>
public static partial class ProductMapping
{
    /// <summary>
    /// Maps a product request to a new Product domain entity via factory Create.
    /// </summary>
    /// <typeparam name="T">The request type deriving from <see cref="ProductRequest"/>.</typeparam>
    /// <param name="request">The request payload with product attributes.</param>
    /// <returns>A result containing the new Product entity or validation failures.</returns>
    public static Result<Product> MapToDomain<T>(this T request) where T : ProductRequest
    {
        return ProductMethod.Create(
        #region Properties
            name: request.Name,
            slug: request.Slug,
            description: request.Description,
            status: request.Status,
        #endregion Properties
        #region SEO
            metaTitle: request.MetaTitle,
            metaDescription: request.MetaDescription,
            metaKeywords: request.MetaKeywords,
        #endregion SEO
        #region Timestamp
            availableOn: request.AvailableOn,
            discontinueOn: request.DiscontinueOn,
            makeActiveAt: request.MakeActiveAt,
        #endregion Timestamp
        #region Fashion
            styleCode: request.StyleCode,
            seasonName: request.SeasonName,
            materialComposition: request.MaterialComposition,
            careInstructions: request.CareInstructions,
            fitNotes: request.FitNotes,
            department: request.Department,
            genderTarget: request.GenderTarget
        #endregion Fashion
        );
    }

    /// <summary>
    /// Applies a product request fields to an existing Product entity via domain Update.
    /// </summary>
    /// <typeparam name="T">The request type deriving from <see cref="ProductRequest"/>.</typeparam>
    /// <param name="request">The request payload with updated product attributes.</param>
    /// <param name="product">The existing product entity to update.</param>
    /// <returns>A result indicating success or validation failures.</returns>
    public static Result MapToDomain<T>(this T request, Product product) where T : ProductRequest
    {
        return product.Update(
        #region Properties
            name: request.Name,
            slug: request.Slug,
            description: request.Description,
            status: request.Status,
        #endregion Properties
        #region SEO
            metaTitle: request.MetaTitle,
            metaDescription: request.MetaDescription,
            metaKeywords: request.MetaKeywords,
        #endregion SEO
        #region Timestamp
            availableOn: request.AvailableOn,
            discontinueOn: request.DiscontinueOn,
            makeActiveAt: request.MakeActiveAt,
        #endregion Timestamp
        #region Fashion
            styleCode: request.StyleCode,
            seasonName: request.SeasonName,
            materialComposition: request.MaterialComposition,
            careInstructions: request.CareInstructions,
            fitNotes: request.FitNotes,
            department: request.Department,
            genderTarget: request.GenderTarget
        #endregion Fashion
            );
    }

    /// <summary>
    /// Maps a Product entity to a detail response DTO with all product attributes.
    /// </summary>
    /// <typeparam name="T">The response type deriving from <see cref="ProductDetailResponse"/>.</typeparam>
    /// <param name="entity">The product domain entity.</param>
    /// <returns>A detail response DTO populated from the entity.</returns>
    public static T MapToDetail<T>(this Product entity) where T : ProductDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            #region Properties
            Name = entity.Name ?? string.Empty,
            Description = entity.Description,
            Status = entity.Status,
            #endregion Properties

            #region SEO
            Slug = entity.Slug ?? string.Empty,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            #endregion SEO

            #region Timestamp
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            MakeActiveAt = entity.MakeActiveAt,
            CreatedAtUtc = entity.CreatedAtUtc,
            #endregion Timestamp
            MasterVariantId = entity.MasterVariantId,

            #region Fashion
            StyleCode = entity.StyleCode,
            SeasonName = entity.SeasonName,
            MaterialComposition = entity.MaterialComposition,
            CareInstructions = entity.CareInstructions,
            FitNotes = entity.FitNotes,
            Department = entity.Department,
            GenderTarget = entity.GenderTarget,
            #endregion Fashion
        };
    }

    /// <summary>
    /// Maps a Product entity to a list-item response DTO, including variant count.
    /// </summary>
    /// <typeparam name="T">The response type deriving from <see cref="ProductListItemResponse"/>.</typeparam>
    /// <param name="entity">The product domain entity.</param>
    /// <returns>A list-item response DTO populated from the entity.</returns>
    public static T MapToListItem<T>(this Product entity) where T : ProductListItemResponse, new()
    {
        return new T
        {
           Id = entity.Id,
            #region Properties
            Name = entity.Name ?? string.Empty,
            Description = entity.Description,
            Status = entity.Status,
            #endregion Properties

            #region SEO
            Slug = entity.Slug ?? string.Empty,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            #endregion SEO

            #region Timestamp
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            MakeActiveAt = entity.MakeActiveAt,
            CreatedAtUtc = entity.CreatedAtUtc,
            #endregion Timestamp

            #region Fashion
            StyleCode = entity.StyleCode,
            SeasonName = entity.SeasonName,
            MaterialComposition = entity.MaterialComposition,
            CareInstructions = entity.CareInstructions,
            FitNotes = entity.FitNotes,
            Department = entity.Department,
            GenderTarget = entity.GenderTarget,
            #endregion Fashion

            #region Relationship
            VariantsCount = entity.Variants.Count,
            ClassificationsCount = entity.Classifications.Count,
            MasterVariantId = entity.MasterVariantId,
            #endregion Relationship
        };
    }
}
