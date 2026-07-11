using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Storefront.OptionTypes.Shared.Mappings;
using Module.Catalog.Features.Storefront.OptionTypes.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Storefront.OptionTypes.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypeStoreMapping")]
public class OptionTypeStoreMappingTests
{
    [Fact(DisplayName = "MapToStoreResponse: Should map OptionType to StoreOptionTypeResponse")]
    public void MapToStoreResponse_ShouldMapEntity()
    {
        var optionType = CreateOptionType();

        var response = optionType.MapToStoreResponse<StoreOptionTypeResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(optionType.Id);
        response.Name.Should().Be(optionType.Name);
        response.Presentation.Should().Be(optionType.Presentation);
        response.Position.Should().Be(optionType.Position);
        response.Values.Should().HaveCount(2);
    }

    [Fact(DisplayName = "MapToStoreValue: Should map OptionValue to StoreOptionValueResponse")]
    public void MapToStoreValue_ShouldMapValue()
    {
        var optionType = CreateOptionType();
        var value = optionType.OptionValues.First();

        var response = value.MapToStoreValue();

        response.Should().NotBeNull();
        response.Id.Should().Be(value.Id);
        response.Name.Should().Be(value.Name);
        response.Presentation.Should().Be(value.Presentation);
        response.Position.Should().Be(value.Position);
    }

    [Fact(DisplayName = "MapToStoreResponse: Should order values by position")]
    public void MapToStoreResponse_ShouldOrderValuesByPosition()
    {
        var optionType = CreateOptionType();

        var response = optionType.MapToStoreResponse<StoreOptionTypeResponse>();

        response.Values.Should().BeInAscendingOrder(v => v.Position);
    }

    private static OptionType CreateOptionType()
    {
        var typeResult = OptionTypeMethod.Create("Color", "Color", position: 1, filterable: true);
        typeResult.IsSuccess.Should().BeTrue();
        var optionType = typeResult.Value;

        var val1Result = OptionValueExtensions.Create(optionType.Id, "Red", "Red", position: 1);
        val1Result.IsSuccess.Should().BeTrue();
        optionType.OptionValues.Add(val1Result.Value);

        var val2Result = OptionValueExtensions.Create(optionType.Id, "Blue", "Blue", position: 2);
        val2Result.IsSuccess.Should().BeTrue();
        optionType.OptionValues.Add(val2Result.Value);

        return optionType;
    }
}
