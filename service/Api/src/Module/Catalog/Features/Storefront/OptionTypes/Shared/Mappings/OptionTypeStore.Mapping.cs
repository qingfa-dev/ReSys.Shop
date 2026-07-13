using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Storefront.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Storefront.OptionTypes.Shared.Mappings;

public static class OptionTypeStoreMapping
{
    public static T MapToStoreResponse<T>(this OptionType entity) where T : StoreOptionTypeResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Presentation = entity.Presentation ?? string.Empty,
            Position = entity.Position,
            Values = entity.OptionValues
                .OrderBy(v => v.Position)
                .Select(v => v.MapToStoreValue())
                .ToList(),
        };
    }

    public static StoreOptionValueResponse MapToStoreValue(this OptionValue value)
    {
        return new StoreOptionValueResponse
        {
            Id = value.Id,
            Name = value.Name ?? string.Empty,
            Presentation = value.Presentation ?? string.Empty,
            Position = value.Position,
        };
    }
}