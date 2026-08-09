using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Wishlists.Shared.Mappings;
using Module.Customer.Features.Storefront.Wishlists.Shared.Models;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistMapping")]
public class WishlistMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map Wishlist to WishlistDetailResponse with items")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var wishlist = CreateWishlist();

        var response = wishlist.MapToDetail<WishlistDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(wishlist.Id);
        response.Name.Should().Be(wishlist.Name);
        response.IsPrivate.Should().Be(wishlist.IsPrivate);
        response.IsDefault.Should().Be(wishlist.IsDefault);
        response.Token.Should().Be(wishlist.Token);
        response.ItemCount.Should().Be(2);
        response.WishedItems.Should().HaveCount(2);
        response.WishedItems.Should().BeInDescendingOrder(i => i.AddedAtUtc);
    }

    [Fact(DisplayName = "MapToDetail: Should map WishedItem properties correctly")]
    public void MapToDetail_ShouldMapWishedItems()
    {
        var wishlist = CreateWishlist();
        var item = wishlist.WishedItems.First();

        var response = wishlist.MapToDetail<WishlistDetailResponse>();
        var mapped = response.WishedItems.First(i => i.Id == item.Id);

        mapped.Should().NotBeNull();
        mapped.Id.Should().Be(item.Id);
        mapped.VariantId.Should().Be(item.VariantId);
        mapped.Quantity.Should().Be(item.Quantity);
        mapped.AddedAtUtc.Should().Be(item.CreatedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map Wishlist to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var wishlist = CreateWishlist();

        var response = wishlist.MapToListItem<WishlistListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(wishlist.Id);
        response.Name.Should().Be(wishlist.Name);
        response.IsPrivate.Should().Be(wishlist.IsPrivate);
        response.ItemCount.Should().Be(2);
    }

    [Fact(DisplayName = "MapToSimple: Should map Wishlist without WishedItems collection")]
    public void MapToSimple_ShouldMapWithoutItems()
    {
        var wishlist = CreateWishlist();

        var response = wishlist.MapToSimple<WishlistDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(wishlist.Id);
        response.Name.Should().Be(wishlist.Name);
        response.IsPrivate.Should().Be(wishlist.IsPrivate);
        response.IsDefault.Should().Be(wishlist.IsDefault);
        response.Token.Should().Be(wishlist.Token);
        response.ItemCount.Should().Be(2);
        response.WishedItems.Should().BeEmpty();
    }

    [Fact(DisplayName = "MapToDetail: Should handle null name")]
    public void MapToDetail_WhenNameIsNull_ShouldMapEmptyString()
    {
        var wishlist = WishlistExtensions.Create("name", Guid.NewGuid()).Value;
        wishlist.Name = null!;

        var response = wishlist.MapToDetail<WishlistDetailResponse>();

        response.Name.Should().Be(string.Empty);
    }

    private static Wishlist CreateWishlist()
    {
        var result = WishlistExtensions.Create("My Wishlist", Guid.NewGuid(), isPrivate: false);
        result.IsSuccess.Should().BeTrue();
        var wishlist = result.Value;

        var add1 = wishlist.AddItem(Guid.NewGuid(), 2);
        add1.IsSuccess.Should().BeTrue();

        var add2 = wishlist.AddItem(Guid.NewGuid(), 1);
        add2.IsSuccess.Should().BeTrue();

        return wishlist;
    }
}
