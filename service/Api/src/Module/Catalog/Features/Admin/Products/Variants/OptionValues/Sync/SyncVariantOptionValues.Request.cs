namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Sync;

public static partial class SyncVariantOptionValues
{
    public sealed record Request
    {
        public IEnumerable<Guid> OptionValueIds { get; init; } = [];
    }
}
