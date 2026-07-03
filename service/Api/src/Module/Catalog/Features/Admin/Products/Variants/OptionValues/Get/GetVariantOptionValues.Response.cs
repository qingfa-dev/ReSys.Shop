namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

public static partial class GetVariantOptionValues
{
    public sealed record Response
    {
        public List<OptionValueItem> Items { get; init; } = [];

        public sealed record OptionValueItem
        {
            public Guid OptionValueId { get; init; }
            public Guid OptionTypeId { get; init; }
            public string OptionTypeName { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public string? Presentation { get; init; }
            public bool IsAssigned { get; init; }
        }
    }
}
