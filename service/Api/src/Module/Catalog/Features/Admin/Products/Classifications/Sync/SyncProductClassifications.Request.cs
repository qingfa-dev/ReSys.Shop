using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Sync;

public static partial class SyncProductClassifications
{
    public sealed record Request : ProductClassificationCollectionParameters;
}