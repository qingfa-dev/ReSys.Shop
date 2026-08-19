using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Options.Sync;

public static partial class SyncProductOptionTypes
{
    public sealed record Request : ProductOptionTypeCollectionParameters;
}