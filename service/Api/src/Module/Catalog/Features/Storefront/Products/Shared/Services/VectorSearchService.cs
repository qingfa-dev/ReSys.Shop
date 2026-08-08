using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Module.Catalog.Features.Storefront.Products.Shared.Services;

public sealed class VectorSearchService : IVectorSearchService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly bool _isNpgsql;

    public VectorSearchService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _isNpgsql = dbContext is ApplicationDbContext appCtx && appCtx.Database.IsNpgsql();
    }

    public async Task<List<Guid>> FindSimilarVariantIdsAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId, CancellationToken cancellationToken)
    {
        var ranked = _isNpgsql
            ? await NpgsqlRankByVariantAsync(queryVector, modelName, topK, excludeProductId, cancellationToken)
            : await InMemoryRankByVariantAsync(queryVector, modelName, topK, excludeProductId, cancellationToken);

        return ranked.Select(r => r.VariantId).ToList();
    }

    public async Task<List<(Guid VariantId, double Score)>> FindSimilarWithScoresAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var ranked = _isNpgsql
            ? await NpgsqlRankByVariantAsync(queryVector, modelName, topK, excludeProductId, cancellationToken)
            : await InMemoryRankByVariantAsync(queryVector, modelName, topK, excludeProductId, cancellationToken);

        return ranked.Select(r => (r.VariantId, Score: 1.0 - r.Distance)).ToList();
    }

    /// <summary>
    /// Ranks variants by their closest "Search"-type image embedding using pgvector's
    /// cosine distance operator (&lt;=&gt;), translated by EF.Functions.CosineDistance.
    /// Equivalent to the SQL "DISTINCT ON (variant_id) ... ORDER BY distance" pattern,
    /// expressed as GroupBy + Min so EF Core can translate it without raw SQL.
    /// </summary>
    private async Task<List<(Guid VariantId, double Distance)>> NpgsqlRankByVariantAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<ImageEmbedding>()
            .Include(e => e.VariantImage)
            .Where(e => e.VariantImage.Type == VariantImageType.Search
                     && e.ModelName == modelName
                     && e.Vector != null
                     && e.VariantImage.VariantId != null);
                     
        if (excludeProductId.HasValue)
            query = query.Where(e => e.VariantImage.Variant!.ProductId != excludeProductId.Value);

        return await query
            .Select(e => new
            {
                VariantId = e.VariantImage.VariantId!.Value,
                Distance = e.Vector!.CosineDistance(queryVector)
            })
            .GroupBy(x => x.VariantId)
            .Select(g => new { VariantId = g.Key, Distance = g.Min(x => x.Distance) })
            .OrderBy(x => x.Distance)
            .Take(topK)
            .Select(x => new ValueTuple<Guid, double>(x.VariantId, x.Distance))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<(Guid VariantId, double Distance)>> InMemoryRankByVariantAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId, CancellationToken cancellationToken)
    {
        var queryArray = queryVector.ToArray();

        var query = _dbContext.Set<ImageEmbedding>()
            .Include(e => e.VariantImage)
                .ThenInclude(vi => vi.Variant)
            .Where(e => e.ModelName == modelName
                     && e.Vector != null
                     && e.VariantImage.Type == VariantImageType.Search
                     && e.VariantImage.VariantId != null);

        if (excludeProductId.HasValue)
            query = query.Where(e => e.VariantImage.Variant!.ProductId != excludeProductId.Value);

        var embeddings = await query.ToListAsync(cancellationToken);

        return embeddings
            .GroupBy(e => e.VariantImage.VariantId!.Value)
            .Select(g => new
            {
                VariantId = g.Key,
                Distance = g.Min(e => CosineDistance(queryArray, e.Vector!.ToArray()))
            })
            .OrderBy(x => x.Distance)
            .Take(topK)
            .Select(x => (x.VariantId, x.Distance))
            .ToList();
    }

    private static double CosineDistance(float[] a, float[] b)
    {
        var dot = 0.0;
        var normA = 0.0;
        var normB = 0.0;
        var minLength = Math.Min(a.Length, b.Length);
        for (var i = 0; i < minLength; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return 1.0 - (dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-12));
    }
}