using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

public static partial class ListVariantsByProduct
{
    public sealed record Response
    {
        public List<Item> Items { get; init; } = [];

        public record Item : VariantDetailResponse;
    }
}