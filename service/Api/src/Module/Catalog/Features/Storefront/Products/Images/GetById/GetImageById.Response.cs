using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Images.Get;

public static partial class GetImageById
{
    public sealed record Response : StoreVariantImageDownloadResponse;
}