using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Assign;

public static partial class AssignProductClassifications
{
    public sealed record Request
    {
        public Guid ProductId { get; init; }
        public IEnumerable<ProductClassificationAssignmentItem> Items { get; init; } = [];
    }
}