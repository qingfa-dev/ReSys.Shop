using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

public static partial class ListVariantsByProduct
{
    // EXCEPTION: collection wrapper — inner Item inherits from VariantDetailResponse
    public sealed record Response
    {
        public List<Item> Items { get; init; } = [];

        public record Item : VariantDetailResponse;
    }
}