using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

public static partial class ProductOptionTypeMapping
{
    public static T MapToListItem<T>(
        this OptionType optionType,
        bool isAssigned,
        int position = 0)
        where T : ProductOptionTypeItemResponse, new()
    {
        return new T
        {
            OptionTypeId = optionType.Id,
            Name = optionType.Name ?? string.Empty,
            Presentation = optionType.Presentation,
            IsAssigned = isAssigned,
            Position = isAssigned ? position : 0
        };
    }
}

public record ProductOptionTypeItemResponse : ProductOptionTypeParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public bool IsAssigned { get; init; }
}
