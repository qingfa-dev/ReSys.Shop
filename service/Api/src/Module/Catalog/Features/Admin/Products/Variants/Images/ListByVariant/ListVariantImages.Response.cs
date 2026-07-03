using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ListByVariant;

public static partial class ListVariantImages
{
    public sealed class Response
    {
        public List<VariantImageDetailResponse> Images { get; init; } = [];
    }
}
