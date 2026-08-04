using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Sync;

public static partial class SyncProductClassifications
{
    public sealed record Request
    {
        public Guid ProductId { get; init; }
        public IEnumerable<ProductClassificationAssignmentItem> Items { get; init; } = [];
    }
}