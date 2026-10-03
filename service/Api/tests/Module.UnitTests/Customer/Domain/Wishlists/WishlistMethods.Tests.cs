using Module.Customer.Domain.Wishlists;
using Module.Customer.Domain.Wishlists.WishedItems;

namespace Module.UnitTests.Profile.Domain.Wishlists;

[Trait("Category", "Unit")]
[Trait("Module", "Profiles")]
[Trait("Feature", "WishlistMethods")]
public class WishlistMethodsTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly string _validName = "My Wishlist";

    #region Create

    [Fact]
    public void Create_WithValidFields_ShouldReturnSuccess()
    {
        var result = WishlistExtensions.Create(_validName, _userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(_validName);
        result.Value.UserId.Should().Be(_userId);
        result.Value.IsPrivate.Should().BeFalse();
        result.Value.IsDefault.Should().Be(WishlistConstant.Defaults.IsDefault);
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.WishedItems.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithPrivateFlag_ShouldSetIsPrivate()
    {
        var result = WishlistExtensions.Create(_validName, _userId, isPrivate: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPrivate.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        var result = WishlistExtensions.Create(string.Empty, _userId);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.Name.Required");
    }

    [Fact]
    public void Create_WithWhiteSpaceName_ShouldReturnFailure()
    {
        var result = WishlistExtensions.Create("   ", _userId);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.Name.Required");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldReturnFailure()
    {
        var longName = new string('a', WishlistConstant.Constraints.MaxNameLength + 1);
        var result = WishlistExtensions.Create(longName, _userId);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.Name.TooLong");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        var result = WishlistExtensions.Create(_validName, Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.UserId.Required");
    }

    #endregion

    #region Update

    [Fact]
    public void Update_ShouldSetName()
    {
        var result = WishlistExtensions.Create(_validName, _userId);
        var newName = "Updated Name";

        var updated = result.Value.Update(name: newName);

        updated.IsSuccess.Should().BeTrue();
        updated.Value.Name.Should().Be(newName);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldReturnFailure()
    {
        var result = WishlistExtensions.Create(_validName, _userId);

        var updated = result.Value.Update(name: string.Empty);

        updated.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldSetIsPrivate()
    {
        var result = WishlistExtensions.Create(_validName, _userId);

        var updated = result.Value.Update(isPrivate: true);

        updated.IsSuccess.Should().BeTrue();
        updated.Value.IsPrivate.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldSetIsDefault()
    {
        var result = WishlistExtensions.Create(_validName, _userId);

        var updated = result.Value.Update(isDefault: true);

        updated.IsSuccess.Should().BeTrue();
        updated.Value.IsDefault.Should().BeTrue();
    }

    #endregion

    #region AddItem

    [Fact]
    public void AddItem_WithValidVariant_ShouldReturnSuccess()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);
        var variantId = Guid.NewGuid();

        var result = wishlist.Value.AddItem(variantId);

        result.IsSuccess.Should().BeTrue();
        result.Value.WishedItems.Should().ContainSingle(i => i.VariantId == variantId);
    }

    [Fact]
    public void AddItem_WithDuplicateVariant_ShouldReturnFailure()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);
        var variantId = Guid.NewGuid();

        wishlist.Value.AddItem(variantId);
        var result = wishlist.Value.AddItem(variantId);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.Item.AlreadyExists");
    }

    [Fact]
    public void AddItem_WhenMaxItemsReached_ShouldReturnFailure()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);
        var initial = wishlist.Value!;

        for (int i = 0; i < WishlistConstant.Constraints.MaxWishedItemsCount; i++)
        {
            initial.WishedItems.Add(new WishedItem
            {
                VariantId = Guid.NewGuid(),
                WishlistId = initial.Id
            });
        }

        var result = wishlist.Value.AddItem(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.MaxItems.Reached");
    }

    #endregion

    #region RemoveItem

    [Fact]
    public void RemoveItem_WithExistingItem_ShouldRemove()
    {
        Result<Wishlist> wishlist = WishlistExtensions.Create(_validName, _userId);
        wishlist = wishlist.Value.AddItem(Guid.NewGuid());
        Guid itemId = wishlist.Value!.WishedItems.First().Id;

        Result<Wishlist> result = wishlist.Value.RemoveItem(itemId);

        result.IsSuccess.Should().BeTrue();
        result.Value.WishedItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WithNonexistentItem_ShouldReturnFailure()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);

        var result = wishlist.Value.RemoveItem(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.Item.NotFound");
    }

    #endregion

    #region Includes

    [Fact]
    public void Includes_WithExistingVariant_ShouldReturnTrue()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);
        var variantId = Guid.NewGuid();
        wishlist.Value.AddItem(variantId);

        var result = wishlist.Value!.Includes(variantId);

        result.Should().BeTrue();
    }

    [Fact]
    public void Includes_WithMissingVariant_ShouldReturnFalse()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);

        var result = wishlist.Value!.Includes(Guid.NewGuid());

        result.Should().BeFalse();
    }

    #endregion

    #region Clear

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId);
        wishlist.Value.AddItem(Guid.NewGuid());
        wishlist.Value.AddItem(Guid.NewGuid());

        var result = wishlist.Value.Clear();

        result.IsSuccess.Should().BeTrue();
        result.Value.WishedItems.Should().BeEmpty();
    }

    #endregion

    #region Share

    [Fact]
    public void Share_ShouldSetIsPrivateToFalse()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId, isPrivate: true);

        var result = wishlist.Value.Share();

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPrivate.Should().BeFalse();
    }

    [Fact]
    public void Share_WhenAlreadyShared_ShouldReturnFailure()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId, isPrivate: false);

        var result = wishlist.Value.Share();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.AlreadyShared");
    }

    #endregion

    #region MakePrivate

    [Fact]
    public void MakePrivate_ShouldSetIsPrivateToTrue()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId, isPrivate: false);

        var result = wishlist.Value.MakePrivate();

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPrivate.Should().BeTrue();
    }

    [Fact]
    public void MakePrivate_WhenAlreadyPrivate_ShouldReturnFailure()
    {
        var wishlist = WishlistExtensions.Create(_validName, _userId, isPrivate: true);

        var result = wishlist.Value.MakePrivate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Wishlist.AlreadyPrivate");
    }

    #endregion

    #region Merge

    [Fact]
    public void Merge_ShouldCombineItems()
    {
        var target = WishlistExtensions.Create("Target", _userId);
        var targetId = target.Value!.Id;
        var other = WishlistExtensions.Create("Other", _userId);
        var otherItem = new WishedItem { VariantId = Guid.NewGuid(), Quantity = 1, WishlistId = other.Value!.Id };
        other.Value.WishedItems.Add(otherItem);

        var result = target.Value.Merge(other.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.WishedItems.Should().ContainSingle(i => i.VariantId == otherItem.VariantId);
    }

    [Fact]
    public void Merge_ShouldDeduplicateByVariantId()
    {
        var target = WishlistExtensions.Create("Target", _userId);
        var targetId = target.Value!.Id;
        var sharedVariantId = Guid.NewGuid();
        target.Value.WishedItems.Add(new WishedItem { VariantId = sharedVariantId, Quantity = 1, WishlistId = targetId });

        var other = WishlistExtensions.Create("Other", _userId);
        other.Value.WishedItems.Add(new WishedItem { VariantId = sharedVariantId, Quantity = 2, WishlistId = other.Value!.Id });

        var result = target.Value.Merge(other.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.WishedItems.Should().ContainSingle(i => i.VariantId == sharedVariantId);
    }

    #endregion
}
