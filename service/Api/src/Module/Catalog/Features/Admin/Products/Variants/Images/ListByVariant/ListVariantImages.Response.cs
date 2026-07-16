using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

public static partial class ListVariantImages
{
    // EXCEPTION: collection wrapper — inner item is VariantImageDetailResponse
    public sealed record Response
    {
        public List<VariantImageDetailResponse> Images { get; init; } = [];
    }
}