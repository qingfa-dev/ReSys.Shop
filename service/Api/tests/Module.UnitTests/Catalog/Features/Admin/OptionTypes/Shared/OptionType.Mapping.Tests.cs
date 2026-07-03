using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Shared;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypes")]
[Trait("Concern", "Mapping")]
public class OptionTypeMappingTests
{
    [Fact(DisplayName = "MapToDomain: Should map OptionTypeRequest to OptionType entity")]
    public void MapToDomain_ShouldMapRequestToEntity()
    {
        // Arrange
        var request = new OptionTypeRequest { Name = "Color", Presentation = "Color Display", Position = 1, Filterable = true };

        // Act
        var result = request.MapToDomain();
        var entity = result.Value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.Presentation.Should().Be(request.Presentation);
        entity.Position.Should().Be(request.Position);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing OptionType entity from request")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        // Arrange
        var request = new OptionTypeRequest { Name = "New Name", Presentation = "New Presentation", Position = 10, Filterable = false };
        var entity = OptionTypeMethod.Create("Old Name", "Old Presentation", 0).Value;

        // Act
        var result = request.MapToDomain(entity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be(request.Name);
        entity.Presentation.Should().Be(request.Presentation);
        entity.Position.Should().Be(request.Position);
        entity.Filterable.Should().Be(request.Filterable);
    }

    [Fact(DisplayName = "MapToDetail: Should map OptionType entity to OptionTypeDetailResponse")]
    public void MapToDetail_ShouldMapEntityToResponse()
    {
        // Arrange
        var entity = OptionTypeMethod.Create("Size", "Size", 1).Value;
        entity.Filterable = true;

        // Act
        var result = entity.MapToDetail<OptionTypeDetailResponse>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Presentation.Should().Be(entity.Presentation);
        result.Position.Should().Be(entity.Position);
        result.Filterable.Should().Be(entity.Filterable);
        result.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        result.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map OptionType entity to OptionTypeListItemResponse")]
    public void MapToListItem_ShouldMapEntityToResponse()
    {
        // Arrange
        var entity = OptionTypeMethod.Create("Material", "Material", 5).Value;
        entity.Filterable = false;

        // Act
        var result = entity.MapToListItem<OptionTypeListItemResponse>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Presentation.Should().Be(entity.Presentation);
        result.Position.Should().Be(entity.Position);
        result.Filterable.Should().Be(entity.Filterable);
        result.OptionValuesCount.Should().Be(entity.OptionValues.Count);
        result.ProductsCount.Should().Be(entity.ProductOptionTypes.Count);
    }
}
