using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;
namespace Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;

public static partial class OptionTypeMapping
{
    public static T MapToDetail<T>(this OptionType entity) where T : OptionTypeDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Presentation = entity.Presentation ?? string.Empty,
            Position = entity.Position,
            Filterable = entity.Filterable,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }

    public static T MapToListItem<T>(this OptionType entity) where T : OptionTypeListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Presentation = entity.Presentation ?? string.Empty,
            Position = entity.Position,
            Filterable = entity.Filterable,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            OptionValuesCount = entity.OptionValues.Count,
            ProductsCount = entity.ProductOptionTypes.Count
        };
    }
}