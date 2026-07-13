using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Assign;

public static partial class AssignProductOptionTypes
{
    public sealed record Request
    {
        public IEnumerable<ProductOptionTypeAssignmentItem> Items { get; init; } = [];
    }
}