using Module.Promotions.Domain.CouponCodes;
using CouponCodeDomain = Module.Promotions.Domain.CouponCodes.CouponCode;

namespace Module.Promotions.Features.Admin.CouponCodes.Shared.Mappings;

/// <summary>Provides mapping methods from request models to CouponCode domain entities.</summary>
public static partial class CouponCodeMapping
{
    /// <summary>Maps a request to a new CouponCode domain entity (create).</summary>
    public static Result<CouponCodeDomain> MapToDomain<T>(this T request) where T : Models.CouponCodeRequest
    {
        return CouponCodeExtensions.Create(
            code: request.Code,
            promotionId: request.PromotionId);
    }
}
