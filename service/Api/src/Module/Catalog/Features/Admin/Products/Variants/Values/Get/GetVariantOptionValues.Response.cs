namespace Module.Catalog.Features.Admin.Products.Variants.Values.Get;

public static partial class GetVariantOptionValues
{
    // EXCEPTION: computed option-value DTO — fields incompatible with OptionValueDetailResponse (different property names + IsAssigned)
    public sealed record Response
    {
        public Guid OptionValueId { get; init; }
        public Guid OptionTypeId { get; init; }
        public string OptionTypeName { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Presentation { get; init; }
        public bool IsAssigned { get; init; }
    }
}
