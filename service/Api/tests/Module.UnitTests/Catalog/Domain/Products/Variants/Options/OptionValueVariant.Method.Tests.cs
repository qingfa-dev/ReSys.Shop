using Module.Catalog.Domain.Products.Variants.Options;

namespace Module.UnitTests.Catalog.Domain.Products.Variants.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "OptionValueVariant")]
public class OptionValueVariantMethodTests
{
    [Fact(DisplayName = "Create: Should return OptionValueVariant with correct properties")]
    public void Create_WithValidParameters_ShouldReturnOptionValueVariant()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var optionValueId = Guid.NewGuid();

        // Act
        var result = OptionValueVariantMethod.Create(variantId, optionValueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.VariantId.Should().Be(variantId);
        result.Value.OptionValueId.Should().Be(optionValueId);
    }
}
