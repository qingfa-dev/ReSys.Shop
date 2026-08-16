using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Shared.Mappings;

public static partial class OptionTypeMapping
{
    public static Result<OptionType> MapToDomain<T>(this T request) where T : OptionTypeRequest
    {
        return OptionTypeMethod.Create(
            name: request.Name,
            presentation: request.Presentation,
            position: request.Position,
            filterable: request.Filterable);
    }

    public static Result MapToDomain<T>(this T request, OptionType optionType) where T : OptionTypeRequest
    {
        return optionType.Update(
            name: request.Name,
            presentation: request.Presentation,
            position: request.Position,
            filterable: request.Filterable);
    }

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
