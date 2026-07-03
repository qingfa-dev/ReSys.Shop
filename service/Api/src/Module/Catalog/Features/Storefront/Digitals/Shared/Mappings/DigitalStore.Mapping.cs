using Module.Catalog.Features.Storefront.Digitals.Shared.Models;

namespace Module.Catalog.Features.Storefront.Digitals.Shared.Mappings;

public static class DigitalStoreMapping
{
    public static T MapToStoreDownload<T>(this (Guid DigitalId, string FileName, string ContentType, string DownloadUrl, DateTimeOffset ExpiresAt, int DownloadCount) source)
        where T : StoreDigitalDownloadResponse, new()
    {
        return new T
        {
            DigitalId = source.DigitalId,
            FileName = source.FileName,
            ContentType = source.ContentType,
            DownloadUrl = source.DownloadUrl,
            ExpiresAt = source.ExpiresAt,
            DownloadCount = source.DownloadCount,
        };
    }
}
