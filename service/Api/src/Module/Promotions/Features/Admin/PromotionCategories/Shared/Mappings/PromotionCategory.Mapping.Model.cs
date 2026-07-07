using PromotionCategoryDomain = Module.Promotions.Domain.PromotionCategories.PromotionCategory;

namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Mappings;

/// <summary>Provides mapping methods from PromotionCategory domain entities to response models.</summary>
public static partial class PromotionCategoryMapping
{
    /// <summary>Maps a domain PromotionCategory to a detail response.</summary>
    public static T MapToDetail<T>(this PromotionCategoryDomain entity) where T : Models.PromotionCategoryDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            Presentation = entity.Presentation,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }

    /// <summary>Maps a domain PromotionCategory to a list item response.</summary>
    public static T MapToListItem<T>(this PromotionCategoryDomain entity) where T : Models.PromotionCategoryListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            Presentation = entity.Presentation,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
