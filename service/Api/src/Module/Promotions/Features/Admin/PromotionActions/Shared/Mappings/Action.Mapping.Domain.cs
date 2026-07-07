using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Features.Admin.PromotionActions.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionActions.Shared.Mappings;

public static class PromotionActionDomainMapping
{
    public static T MapToDetail<T>(this PromotionAction entity) where T : PromotionActionDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Type = entity.Type,
            Preferences = entity.Preferences,
            CalculatorType = entity.CalculatorType,
            PromotionId = entity.PromotionId,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    public static T MapToListItem<T>(this PromotionAction entity) where T : PromotionActionListResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Type = entity.Type,
            Preferences = entity.Preferences,
            CalculatorType = entity.CalculatorType,
        };
    }
}
