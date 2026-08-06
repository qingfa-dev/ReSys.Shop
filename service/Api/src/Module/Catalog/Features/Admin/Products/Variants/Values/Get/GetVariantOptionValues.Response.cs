using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Values.Get;

public static partial class GetVariantOptionValues
{
    // EXCEPTION: computed option-value DTO — fields incompatible with OptionValueDetailResponse (different property names + IsAssigned)
    public sealed record Response : OptionValueParameters
    {
        public Guid OptionValueId { get; init; }
        public Guid OptionTypeId { get; init; }
        public string OptionTypeName { get; init; } = string.Empty;
        public bool IsAssigned { get; init; }
    }
}
