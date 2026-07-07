using Module.Promotions.Domain.PromotionCategories;
using PromotionCategoryDomain = Module.Promotions.Domain.PromotionCategories.PromotionCategory;

namespace Module.Promotions.Features.Admin.PromotionCategories.Shared.Mappings;

/// <summary>Provides mapping methods from request models to PromotionCategory domain entities.</summary>
public static partial class PromotionCategoryMapping
{
    /// <summary>Maps a request to a new PromotionCategory domain entity (create).</summary>
    public static Result<PromotionCategoryDomain> MapToDomain<T>(this T request) where T : Models.PromotionCategoryRequest
    {
        return PromotionCategoryExtensions.Create(
            name: request.Name,
            code: request.Code,
            presentation: request.Presentation);
    }

    /// <summary>Maps a partial-update request (PATCH) to an existing PromotionCategory domain entity.</summary>
    public static Result MapUpdateToDomain<T>(this T request, PromotionCategoryDomain category) where T : Models.PromotionCategoryUpdateRequest
    {
        return category.Update(
            name: request.Name,
            code: request.Code,
            presentation: request.Presentation);
    }
}
