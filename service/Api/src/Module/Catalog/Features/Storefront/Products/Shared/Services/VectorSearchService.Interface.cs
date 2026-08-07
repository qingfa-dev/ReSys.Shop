using Pgvector;

namespace Module.Catalog.Features.Storefront.Products.Shared.Services;

public interface IVectorSearchService
{
    Task<List<Guid>> FindSimilarVariantIdsAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId, CancellationToken cancellationToken);

    Task<List<(Guid VariantId, double Score)>> FindSimilarWithScoresAsync(
        Vector queryVector, string modelName, int topK,
        CancellationToken cancellationToken = default);
}
