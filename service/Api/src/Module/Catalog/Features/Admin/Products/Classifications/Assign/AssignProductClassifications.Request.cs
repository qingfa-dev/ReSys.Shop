using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Classifications.Assign;

public static partial class AssignProductClassifications
{
    public sealed record Request
    {
        public IEnumerable<ProductClassificationAssignmentItem> Items { get; init; } = [];
    }
}