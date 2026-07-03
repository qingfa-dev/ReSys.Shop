using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductOptionTypeMapping")]
public class ProductOptionTypeMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map assignment item to domain entity")]
    public void ToDomain_ShouldMapItemToEntity()
    {
        var productId = Guid.NewGuid();
        var item = new ProductOptionTypeAssignmentItem
        {
            OptionTypeId = Guid.NewGuid(),
            Position = 1,
        };

        var result = item.MapToDomain(productId);
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.ProductId.Should().Be(productId);
        entity.OptionTypeId.Should().Be(item.OptionTypeId);
        entity.Position.Should().Be(item.Position);
    }

    [Fact(DisplayName = "ToDomain (Update): Should update position on existing entity")]
    public void ToDomain_Update_ShouldUpdatePosition()
    {
        var entity = ProductOptionTypeMethod.Create(
            Guid.NewGuid(), Guid.NewGuid(), position: 0).Value;

        var item = new ProductOptionTypeAssignmentItem
        {
            OptionTypeId = Guid.NewGuid(),
            Position = 5,
        };

        item.MapToDomain(entity);

        entity.Position.Should().Be(5);
    }

    [Fact(DisplayName = "ToListItem: Should map option type to list item when assigned")]
    public void ToListItem_WhenAssigned_ShouldMapCorrectly()
    {
        var optionType = new OptionType
        {
            Id = Guid.NewGuid(),
            Name = "Color",
            Presentation = "Color Variant",
        };

        var response = optionType.MapToListItem<ProductOptionTypeItemResponse>(
            isAssigned: true, position: 2);

        response.Should().NotBeNull();
        response.OptionTypeId.Should().Be(optionType.Id);
        response.Name.Should().Be(optionType.Name);
        response.Presentation.Should().Be(optionType.Presentation);
        response.IsAssigned.Should().BeTrue();
        response.Position.Should().Be(2);
    }

    [Fact(DisplayName = "ToListItem: Should map option type when not assigned with zero position")]
    public void ToListItem_WhenNotAssigned_ShouldMapWithZeroPosition()
    {
        var optionType = new OptionType
        {
            Id = Guid.NewGuid(),
            Name = "Size",
            Presentation = null,
        };

        var response = optionType.MapToListItem<ProductOptionTypeItemResponse>(
            isAssigned: false, position: 10);

        response.IsAssigned.Should().BeFalse();
        response.Position.Should().Be(0);
        response.OptionTypeId.Should().Be(optionType.Id);
    }

    [Fact(DisplayName = "ToListItem: Should handle null Presentation")]
    public void ToListItem_WhenPresentationIsNull_ShouldMapCorrectly()
    {
        var optionType = new OptionType
        {
            Id = Guid.NewGuid(),
            Name = "Material",
            Presentation = null,
        };

        var response = optionType.MapToListItem<ProductOptionTypeItemResponse>(
            isAssigned: true, position: 3);

        response.Presentation.Should().BeNull();
        response.Name.Should().Be("Material");
    }
}
