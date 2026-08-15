using Module.Catalog.Features.Storefront.Products.Images.Search.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Images.Search;

public static partial class SearchByImage
{
    public sealed record Request : ImageSearchParameters;
}
