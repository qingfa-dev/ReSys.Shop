using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

public static partial class OptionValueMapping
{
    public static Result<OptionValue> MapToDomain<T>(this T request, Guid optionTypeId) where T : OptionValueRequest
    {
        return OptionValueMethod.Create(
            optionTypeId,
            request.Name,
            request.Presentation ?? string.Empty,
            request.Position);
    }

    public static Result MapToDomain<T>(this T request, OptionValue entity) where T : OptionValueRequest
    {
        return entity.Update(
            name: request.Name,
            presentation: request.Presentation ?? string.Empty,
            position: request.Position);
    }

}