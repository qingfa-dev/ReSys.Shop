using Microsoft.CodeAnalysis;

using Module.Profile.Domain.Wishlists.WishedItems;

namespace Module.Profile.Domain.Wishlists;

public static class WishlistExtensions
{
    #region Factory Methods

    public static Result<Wishlist> Create(
        string name,
        Guid userId,
        bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return WishlistResult.Failure.NameRequired;

        if (name.Length > WishlistConstant.Constraints.MaxNameLength)
            return WishlistResult.Failure.NameTooLong;

        if (userId == Guid.Empty)
            return WishlistResult.Failure.UserIdRequired;

        return new Wishlist
        {
            Name = name,
            Token = GenerateToken(),
            IsDefault = WishlistConstant.Defaults.IsDefault,
            IsPrivate = isPrivate,
            UserId = userId,
            WishedItems = []
        };
    }

    private static string GenerateToken()
    {
        var bytes = new byte[WishlistConstant.Defaults.TokenLength];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    #endregion

    #region Update

    public static Result<Wishlist> Update(
        this Wishlist wishlist,
        string? name = default,
        bool? isPrivate = default,
        bool? isDefault = default)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return WishlistResult.Failure.NameRequired;
            if (name.Length > WishlistConstant.Constraints.MaxNameLength)
                return WishlistResult.Failure.NameTooLong;
            wishlist.Name = name;
        }

        if (isPrivate.HasValue) wishlist.IsPrivate = isPrivate.Value;
        if (isDefault.HasValue) wishlist.IsDefault = isDefault.Value;

        return wishlist;
    }

    #endregion

    #region Business Logic

    public static Result<Wishlist> AddItem(this Wishlist wishlist, Guid variantId, int quantity = 1)
    {
        if (wishlist.WishedItems.Count >= WishlistConstant.Constraints.MaxWishedItemsCount)
            return WishlistResult.Failure.MaxItemsReached;
        if (wishlist.WishedItems.Any(i => i.VariantId == variantId))
            return WishlistResult.Failure.ItemAlreadyExists;
        var itemResult = WishedItemMethod.Create(variantId, wishlist.Id, quantity);
        if (itemResult.IsFailure) return itemResult.Errors;
        wishlist.WishedItems.Add(itemResult.Value);
        return wishlist;
    }

    public static Result<Wishlist> RemoveItem(this Wishlist wishlist, Guid wishedItemId)
    {
        var item = wishlist.WishedItems.FirstOrDefault(i => i.Id == wishedItemId);
        if (item is null) return WishlistResult.Failure.ItemNotFound;
        wishlist.WishedItems.Remove(item);
        return wishlist;
    }

    public static bool Includes(this Wishlist wishlist, Guid variantId)
    {
        return wishlist.WishedItems.Any(i => i.VariantId == variantId);
    }

    public static Result<Wishlist> Clear(this Wishlist wishlist)
    {
        wishlist.WishedItems.Clear();
        return wishlist;
    }

    public static Result<Wishlist> Share(this Wishlist wishlist)
    {
        if (!wishlist.IsPrivate) return WishlistResult.Failure.AlreadyShared;
        wishlist.IsPrivate = false;
        return wishlist;
    }

    public static Result<Wishlist> MakePrivate(this Wishlist wishlist)
    {
        if (wishlist.IsPrivate) return WishlistResult.Failure.AlreadyPrivate;
        wishlist.IsPrivate = true;
        return wishlist;
    }

    public static Result<Wishlist> Merge(this Wishlist targetList, Wishlist other)
    {
        foreach (var item in other.WishedItems)
        {
            if (targetList.WishedItems.Count >= WishlistConstant.Constraints.MaxWishedItemsCount) break;
            if (!targetList.WishedItems.Any(i => i.VariantId == item.VariantId))
            {
                targetList.WishedItems.Add(new WishedItem
                {
                    VariantId = item.VariantId, Quantity = item.Quantity, WishlistId = targetList.Id
                });
            }
        }

        return targetList;
    }

    #endregion
}