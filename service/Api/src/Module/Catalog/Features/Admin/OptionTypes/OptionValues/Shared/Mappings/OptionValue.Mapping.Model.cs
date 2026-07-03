using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

public static partial class OptionValueMapping
{
    public static T MapToListItem<T>(this OptionValue entity) where T : OptionValueListItemResponse, new()
    {
        // Map: Entity properties to the specified response DTO type
        return new T
        {
            Id = entity.Id,
            OptionTypeId = entity.OptionTypeId,
            Name = entity.Name ?? string.Empty,
            Presentation = entity.Presentation ?? string.Empty,
            Position = entity.Position,
            OptionTypeName = entity.OptionType.Name,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc
        };
    }

    public static T MapToDetail<T>(this OptionValue entity) where T : OptionValueDetailResponse, new()
    {
        // Map: Entity properties to the specified response DTO type
        return new T
        {
            Id = entity.Id,
            OptionTypeId = entity.OptionTypeId,
            Name = entity.Name ?? string.Empty,
            Presentation = entity.Presentation ?? string.Empty,
            Position = entity.Position,
            OptionTypeName = entity.OptionType.Name,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc
        };
    }
}