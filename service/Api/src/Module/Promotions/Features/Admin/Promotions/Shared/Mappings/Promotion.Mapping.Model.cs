using PromotionDomain = Module.Promotions.Domain.Promotions.Promotion;

namespace Module.Promotions.Features.Admin.Promotions.Shared.Mappings;

/// <summary>Provides mapping methods from Promotion domain entities to response models.</summary>
public static partial class PromotionMapping
{
    /// <summary>Maps a domain Promotion to a detail response.</summary>
    public static T MapToDetail<T>(this PromotionDomain entity) where T : Models.PromotionDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            Description = entity.Description,
            UsageLimit = entity.UsageLimit,
            PerCustomerUsageLimit = entity.PerCustomerUsageLimit,
            StartsAtUtc = entity.StartsAtUtc,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            MatchPolicy = entity.MatchPolicy,
            Kind = entity.Kind,
            Advertise = entity.Advertise,
            Active = entity.Active,
            Position = entity.Position,
            Path = entity.Path,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            DeletedAtUtc = entity.DeletedAtUtc,
            IsDeleted = entity.IsDeleted,
        };
    }

    /// <summary>Maps a domain Promotion to a list item response.</summary>
    public static T MapToListItem<T>(this PromotionDomain entity) where T : Models.PromotionListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            Description = entity.Description,
            UsageLimit = entity.UsageLimit,
            PerCustomerUsageLimit = entity.PerCustomerUsageLimit,
            StartsAtUtc = entity.StartsAtUtc,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            MatchPolicy = entity.MatchPolicy,
            Kind = entity.Kind,
            Advertise = entity.Advertise,
            Active = entity.Active,
            Position = entity.Position,
            Path = entity.Path,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            DeletedAtUtc = entity.DeletedAtUtc,
            IsDeleted = entity.IsDeleted,
        };
    }
}
