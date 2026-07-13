using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Sync;

public static partial class SyncProductOptionTypes
{
    public sealed record Request
    {
        public IEnumerable<ProductOptionTypeAssignmentItem> Items { get; init; } = [];
    }
}