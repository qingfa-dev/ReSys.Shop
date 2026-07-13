using Module.Profile.Domain.Wishlists;
using Module.Profile.Domain.Wishlists.WishedItems;
using Module.Profile.Features.Store.Wishlists.Shared.Models;

namespace Module.Profile.Features.Store.Wishlists.Shared.Mappings;

public static partial class WishlistMapping
{
    public static T MapToDetail<T>(this Wishlist entity) where T : WishlistDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            IsPrivate = entity.IsPrivate,
            IsDefault = entity.IsDefault,
            Token = entity.Token,
            ItemCount = entity.WishedItems.Count,
            WishedItems = entity.WishedItems
                .OrderByDescending(i => i.CreatedAtUtc)
                .Select(i => new WishedItemResponse
                {
                    Id = i.Id,
                    VariantId = i.VariantId,
                    Quantity = i.Quantity,
                    AddedAtUtc = i.CreatedAtUtc
                })
                .ToList()
        };
    }

    public static T MapToListItem<T>(this Wishlist entity) where T : WishlistListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            IsPrivate = entity.IsPrivate,
            ItemCount = entity.WishedItems.Count
        };
    }

    public static T MapToSimple<T>(this Wishlist entity) where T : WishlistDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            IsPrivate = entity.IsPrivate,
            IsDefault = entity.IsDefault,
            Token = entity.Token,
            ItemCount = entity.WishedItems.Count
        };
    }
}