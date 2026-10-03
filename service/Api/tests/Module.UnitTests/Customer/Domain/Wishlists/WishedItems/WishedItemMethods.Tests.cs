using Module.Customer.Domain.Wishlists.WishedItems;

namespace Module.UnitTests.Profile.Domain.Wishlists.WishedItems;

[Trait("Category", "Unit")]
[Trait("Module", "Profiles")]
[Trait("Feature", "WishedItemMethods")]
public class WishedItemMethodsTests
{
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _wishlistId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidFields_ShouldReturnSuccess()
    {
        Result<WishedItem> result = WishedItemMethod.Create(_variantId, _wishlistId, 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.VariantId.Should().Be(_variantId);
        result.Value.WishlistId.Should().Be(_wishlistId);
        result.Value.Quantity.Should().Be(2);
    }

    [Fact]
    public void Create_WithDefaultQuantity_ShouldSetToOne()
    {
        Result<WishedItem> result = WishedItemMethod.Create(_variantId, _wishlistId);

        result.Value.Quantity.Should().Be(WishedItemConstant.Defaults.Quantity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Create_WithInvalidVariantId_ShouldReturnFailure(string? variantIdStr)
    {
        Guid id = variantIdStr is not null ? Guid.Parse(variantIdStr) : Guid.Empty;
        Result<WishedItem> result = WishedItemMethod.Create(id, _wishlistId);

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Create_WithInvalidQuantity_ShouldReturnFailure(int quantity)
    {
        Result<WishedItem> result = WishedItemMethod.Create(_variantId, _wishlistId, quantity);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldSetQuantity()
    {
        Result<WishedItem> result = WishedItemMethod.Create(_variantId, _wishlistId);

        Result<WishedItem> updated = result.Value.Update(quantity: 5);

        updated.Value.Quantity.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void Update_WithInvalidQuantity_ShouldReturnFailure(int quantity)
    {
        Result<WishedItem> result = WishedItemMethod.Create(_variantId, _wishlistId);

        Result<WishedItem> updated = result.Value.Update(quantity: quantity);

        updated.IsFailure.Should().BeTrue();
    }

}
