using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionValueMapping")]
public class OptionValueMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var optionTypeId = Guid.NewGuid();
        var request = new OptionValueRequest
        {
            Name = "Red",
            Presentation = "Red Color",
            Position = 1,
        };

        var result = request.MapToDomain(optionTypeId);
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.OptionTypeId.Should().Be(optionTypeId);
        entity.Name.Should().Be(request.Name);
        entity.Presentation.Should().Be(request.Presentation);
        entity.Position.Should().Be(request.Position);
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "ToDomain (Update): Should update existing entity from request")]
    public void ToDomain_Update_ShouldUpdateEntity()
    {
        var entity = OptionValueMethod.Create(
            Guid.NewGuid(), "Old", "Old Color", 0).Value;

        var request = new OptionValueRequest
        {
            Name = "Updated",
            Presentation = "Updated Color",
            Position = 5,
        };

        var result = request.MapToDomain(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Name.Should().Be("Updated");
        entity.Presentation.Should().Be("Updated Color");
        entity.Position.Should().Be(5);
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var entity = CreateOptionValue();

        var response = entity.MapToDetail<OptionValueDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.OptionTypeId.Should().Be(entity.OptionTypeId);
        response.Name.Should().Be(entity.Name);
        response.Presentation.Should().Be(entity.Presentation);
        response.Position.Should().Be(entity.Position);
        response.OptionTypeName.Should().Be(entity.OptionType.Name);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "ToDetail: Should handle null Name as empty string")]
    public void ToDetail_WhenNameIsNull_ShouldUseEmptyString()
    {
        var entity = CreateOptionValue(e =>
        {
            e.Name = null!;
            e.Presentation = null;
        });

        var response = entity.MapToDetail<OptionValueDetailResponse>();

        response.Name.Should().BeEmpty();
        response.Presentation.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var entity = CreateOptionValue();

        var response = entity.MapToListItem<OptionValueListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.OptionTypeId.Should().Be(entity.OptionTypeId);
        response.Name.Should().Be(entity.Name);
        response.Presentation.Should().Be(entity.Presentation);
        response.Position.Should().Be(entity.Position);
        response.OptionTypeName.Should().Be(entity.OptionType.Name);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "ToListItem: Should handle edge case with negative position")]
    public void ToListItem_WhenPositionIsNegative_ShouldMapCorrectly()
    {
        var entity = CreateOptionValue(e =>
        {
            e.Position = -1;
            e.Name = "Hidden";
            e.Presentation = null;
        });

        var response = entity.MapToListItem<OptionValueListItemResponse>();

        response.Position.Should().Be(-1);
        response.Name.Should().Be("Hidden");
        response.Presentation.Should().BeEmpty();
    }

    private static OptionValue CreateOptionValue(Action<OptionValue>? configure = null)
    {
        var optionType = new OptionType
        {
            Id = Guid.NewGuid(),
            Name = "Color",
        };

        var entity = new OptionValue
        {
            Id = Guid.NewGuid(),
            OptionTypeId = optionType.Id,
            Name = "Red",
            Presentation = "Red Color",
            Position = 1,
            OptionType = optionType,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
        };
        configure?.Invoke(entity);
        return entity;
    }
}
