using CouponCodeDomain = Module.Promotions.Domain.CouponCodes.CouponCode;

namespace Module.Promotions.Features.Admin.CouponCodes.Shared.Mappings;

/// <summary>Provides mapping methods from CouponCode domain entities to response models.</summary>
public static partial class CouponCodeMapping
{
    /// <summary>Maps a domain CouponCode to a detail response.</summary>
    public static T MapToDetail<T>(this CouponCodeDomain entity) where T : Models.CouponCodeDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Code = entity.Code ?? string.Empty,
            PromotionId = entity.PromotionId,
            State = entity.State.ToString(),
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    /// <summary>Maps a domain CouponCode to a list item response.</summary>
    public static T MapToListItem<T>(this CouponCodeDomain entity) where T : Models.CouponCodeListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Code = entity.Code ?? string.Empty,
            PromotionId = entity.PromotionId,
            State = entity.State.ToString(),
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
