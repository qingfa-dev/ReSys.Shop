using Module.Promotions.Domain.Promotions;
using PromotionDomain = Module.Promotions.Domain.Promotions.Promotion;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

/// <summary>Provides mapping methods from request models to Promotion domain entities.</summary>
public static partial class PromotionMapping
{
    /// <summary>Maps a request to a new Promotion domain entity (create).</summary>
    public static Result<PromotionDomain> MapToDomain<T>(this T request) where T : Models.PromotionRequest
    {
        return PromotionExtensions.Create(
            name: request.Name,
            code: request.Code,
            description: request.Description,
            usageLimit: request.UsageLimit,
            perCustomerUsageLimit: request.PerCustomerUsageLimit,
            startsAtUtc: request.StartsAtUtc,
            expiresAtUtc: request.ExpiresAtUtc,
            matchPolicy: request.MatchPolicy,
            kind: request.Kind,
            advertise: request.Advertise,
            active: request.Active,
            position: request.Position,
            path: request.Path);
    }

    /// <summary>Maps a full-update request to an existing Promotion domain entity (PUT semantics).</summary>
    public static Result MapToDomain<T>(this T request, PromotionDomain promotion) where T : Models.PromotionRequest
    {
        return promotion.Update(
            name: request.Name,
            code: request.Code,
            description: request.Description,
            usageLimit: request.UsageLimit,
            perCustomerUsageLimit: request.PerCustomerUsageLimit,
            startsAtUtc: request.StartsAtUtc,
            expiresAtUtc: request.ExpiresAtUtc,
            matchPolicy: request.MatchPolicy,
            kind: request.Kind,
            advertise: request.Advertise,
            active: request.Active,
            position: request.Position,
            path: request.Path);
    }

    /// <summary>Maps a partial-update request (PATCH) to an existing Promotion domain entity.</summary>
    public static Result MapUpdateToDomain<T>(this T request, PromotionDomain promotion) where T : Models.PromotionUpdateRequest
    {
        return promotion.Update(
            name: request.Name,
            code: request.Code,
            description: request.Description,
            usageLimit: request.UsageLimit,
            perCustomerUsageLimit: request.PerCustomerUsageLimit,
            startsAtUtc: request.StartsAtUtc,
            expiresAtUtc: request.ExpiresAtUtc,
            matchPolicy: request.MatchPolicy,
            kind: request.Kind,
            advertise: request.Advertise,
            active: request.Active,
            position: request.Position,
            path: request.Path);
    }
}
