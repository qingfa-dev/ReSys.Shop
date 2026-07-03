using Module.Catalog.Domain.Products.Options;

namespace Module.UnitTests.Catalog.Domain.Products.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "ProductOptionType")]
public class ProductOptionTypeMethodTests
{
    [Fact(DisplayName = "Create: Should return ProductOptionType with correct properties")]
    public void Create_WithValidParameters_ShouldReturnProductOptionType()
    {
        var productId = Guid.NewGuid();
        var optionTypeId = Guid.NewGuid();
        var position = 5;

        var result = ProductOptionTypeMethod.Create(productId, optionTypeId, position);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProductId.Should().Be(productId);
        result.Value.OptionTypeId.Should().Be(optionTypeId);
        result.Value.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Create: With default position should be zero")]
    public void Create_WithDefaultPosition_ShouldBeZero()
    {
        var result = ProductOptionTypeMethod.Create(Guid.NewGuid(), Guid.NewGuid());

        result.Value.Position.Should().Be(0);
    }
}
