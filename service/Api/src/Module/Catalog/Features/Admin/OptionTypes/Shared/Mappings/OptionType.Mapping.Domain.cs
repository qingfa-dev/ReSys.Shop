using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;

public static partial class OptionTypeMapping
{
    public static Result<OptionType> MapToDomain<T>(this T request) where T : OptionTypeRequest
    {
        return OptionTypeMethod.Create(
            name: request.Name,
            presentation: request.Presentation,
            position: request.Position);
    }

    public static Result MapToDomain<T>(this T request, OptionType optionType) where T : OptionTypeRequest
    {
        return optionType.Update(
            name: request.Name,
            presentation: request.Presentation,
            position: request.Position,
            filterable: request.Filterable);
    }
}