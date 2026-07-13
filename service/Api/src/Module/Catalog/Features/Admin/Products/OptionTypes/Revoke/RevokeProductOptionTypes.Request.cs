using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Revoke;

public static partial class RevokeProductOptionTypes
{
    public sealed record Request
    {
        public IEnumerable<ProductOptionTypeAssignmentItem> Items { get; init; } = [];
    }
}