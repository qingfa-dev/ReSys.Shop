using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Storefront.Options.Shared.Mappings;
using Module.Catalog.Features.Storefront.Options.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.OptionTypes.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypeStoreMapping")]
public class OptionTypeStoreMappingTests
{
    [Fact(DisplayName = "MapToStoreListItem: Should map OptionType to StoreOptionTypeListItem")]
    public void MapToStoreListItem_ShouldMapEntity()
    {
        var optionType = CreateOptionType();

        var response = optionType.MapToStoreListItem<StoreOptionTypeListItem>();

        response.Should().NotBeNull();
        response.Id.Should().Be(optionType.Id);
        response.Name.Should().Be(optionType.Name);
        response.Presentation.Should().Be(optionType.Presentation);
        response.Position.Should().Be(optionType.Position);
    }

    [Fact(DisplayName = "MapToStoreListItem: Should map OptionValue to StoreOptionValueListItemResponse")]
    public void MapToStoreListItem_ShouldMapValue()
    {
        var optionType = CreateOptionType();
        var value = optionType.OptionValues.First();

        var response = value.MapToStoreListItem<StoreOptionValueListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(value.Id);
        response.Name.Should().Be(value.Name);
        response.Presentation.Should().Be(value.Presentation);
        response.Position.Should().Be(value.Position);
    }

    private static OptionType CreateOptionType()
    {
        var typeResult = OptionTypeMethod.Create("Color", "Color", position: 1, filterable: true);
        typeResult.IsSuccess.Should().BeTrue();
        var optionType = typeResult.Value;

        var val1Result = OptionValueMethod.Create(optionType.Id, "Red", "Red", position: 1);
        val1Result.IsSuccess.Should().BeTrue();
        val1Result.Value.OptionType = optionType;
        optionType.OptionValues.Add(val1Result.Value);

        var val2Result = OptionValueMethod.Create(optionType.Id, "Blue", "Blue", position: 2);
        val2Result.IsSuccess.Should().BeTrue();
        val2Result.Value.OptionType = optionType;
        optionType.OptionValues.Add(val2Result.Value);

        return optionType;
    }
}
