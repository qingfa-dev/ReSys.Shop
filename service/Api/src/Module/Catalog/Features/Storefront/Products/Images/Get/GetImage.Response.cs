using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    // EXCEPTION: image serving response — no domain entity
    public sealed record Response : StoreVariantImageDownloadResponse;
}