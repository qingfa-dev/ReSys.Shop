namespace Module.Catalog.Features.Storefront.Digitals.Shared.Models;

public class StoreDigitalDownloadResponse
{
    public Guid DigitalId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public int DownloadCount { get; init; }
}
