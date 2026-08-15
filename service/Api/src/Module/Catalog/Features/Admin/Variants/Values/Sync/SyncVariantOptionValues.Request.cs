using Module.Catalog.Features.Admin.Variants.Values.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Values.Sync;

public static partial class SyncVariantOptionValues
{
    public sealed record Request : VariantOptionValueCollectionParameters;
}