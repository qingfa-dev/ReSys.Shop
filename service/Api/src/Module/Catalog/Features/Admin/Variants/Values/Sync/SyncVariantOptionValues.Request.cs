namespace Module.Catalog.Features.Admin.Variants.Values.Sync;

public static partial class SyncVariantOptionValues
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public IEnumerable<Guid> OptionValueIds { get; init; } = [];
    }
}