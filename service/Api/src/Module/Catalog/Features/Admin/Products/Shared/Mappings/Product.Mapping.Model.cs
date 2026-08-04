using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Shared.Mappings;

/// <summary>
/// Maps between Product domain entities and response DTOs.
/// </summary>
public static partial class ProductMapping
{
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