using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Classifications.Sync;

public static partial class SyncProductClassifications
{
    public sealed record Request
    {
        public IEnumerable<ProductClassificationAssignmentItem> Items { get; init; } = [];
    }
}