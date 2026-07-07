using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionRules.Shared.Mappings;

public static class PromotionRuleDomainMapping
{
    public static T MapToDetail<T>(this PromotionRule entity) where T : PromotionRuleDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Type = entity.Type,
            Preferences = entity.Preferences,
            PromotionId = entity.PromotionId,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    public static T MapToListItem<T>(this PromotionRule entity) where T : PromotionRuleListResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Type = entity.Type,
            Preferences = entity.Preferences,
        };
    }
}
