using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class StoreVariantImageMapping
{
    public static T MapToStoreListItem<T>(this VariantImage image) where T : StoreVariantImageListItemResponse, new()
    {
        var baseResponse = image.MapToDetail<T>();
        return baseResponse;
    }

    public static T MapToStoreDownloadItem<T>(this VariantImage image, Stream stream) where T : StoreVariantImageDownloadResponse, new()
    {
        var baseResponse = image.MapToDownload<T>(stream);
        return baseResponse;
    }
}