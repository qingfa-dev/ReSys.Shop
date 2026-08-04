using Pgvector;

namespace Module.Catalog.Features.Storefront.Products.Shared.Services;

public interface IVectorSearchService
{
    Task<List<Guid>> FindSimilarVariantIdsAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId, CancellationToken cancellationToken);
}
