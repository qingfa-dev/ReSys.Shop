using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Classifications.Revoke;

public static partial class RevokeProductClassifications
{
    public sealed record Request
    {
        public IEnumerable<ProductClassificationAssignmentItem> Items { get; init; } = [];
    }
}