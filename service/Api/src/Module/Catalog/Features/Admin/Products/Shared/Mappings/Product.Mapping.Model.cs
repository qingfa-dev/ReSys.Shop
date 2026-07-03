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
            Name = entity.Name ?? string.Empty,
            Slug = entity.Slug ?? string.Empty,
            Description = entity.Description,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            Status = entity.Status,
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            MasterVariantId = entity.MasterVariantId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
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
            Name = entity.Name ?? string.Empty,
            Slug = entity.Slug ?? string.Empty,
            Description = entity.Description,
            Status = entity.Status,
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            MasterVariantId = entity.MasterVariantId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            VariantsCount = entity.Variants.Count,
        };
    }
}
