using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Storefront.OptionTypes.Shared.Models;

public record StoreOptionTypeResponse : OptionTypeParameters
{
    public Guid Id { get; init; }
    public List<StoreOptionValueResponse> Values { get; init; } = [];
}

public record StoreOptionValueResponse : OptionValueParameters
{
    public Guid Id { get; init; }
}