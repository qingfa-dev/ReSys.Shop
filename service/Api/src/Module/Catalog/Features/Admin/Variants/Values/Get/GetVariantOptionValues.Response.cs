using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Values.Get;

public static partial class GetVariantOptionValues
{
    public sealed record Response : OptionValueParameters
    {
        public Guid OptionValueId { get; init; }
        public Guid OptionTypeId { get; init; } = Guid.Empty;
        public string OptionTypeName { get; init; } = string.Empty;
        public bool IsAssigned { get; init; }
    }
}
