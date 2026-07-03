using Module.Catalog.Features.Storefront.Digitals.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Digitals.Get.DownloadLink;

/// <summary>
/// Defines the use case for generating a digital download link.
/// </summary>
public static partial class GenerateDownloadLink
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler
        : IQueryHandler<Query, Response>
    {
        /// <summary>
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="query">The query containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=query!=null, post=result!=null
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var downloadUrl = $"/api/files/digitals/{query.Id}/download?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

            var source = (
                DigitalId: query.Id,
                FileName: "download",
                ContentType: "application/octet-stream",
                DownloadUrl: downloadUrl,
                ExpiresAt: expiresAt,
                DownloadCount: 0
            );

            return Result<Response>.Ok(
                source.MapToStoreDownload<Response>());
        }
    }
}
