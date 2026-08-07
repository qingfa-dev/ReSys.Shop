using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

using Pgvector;

using Shared.Operational.Persistence.Data;

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
        if (_isNpgsql)
        {
            return await NpgsqlSearchAsync(queryVector, modelName, topK, excludeProductId, cancellationToken);
        }
        else
        {
            return await InMemorySearchAsync(queryVector, modelName, topK, excludeProductId, cancellationToken);
        }
    }

    private async Task<List<Guid>> NpgsqlSearchAsync(
        Vector queryVector, string modelName, int topK,
        Guid? excludeProductId, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT DISTINCT ON (v.id) v.*
            FROM catalog.variants v
            INNER JOIN catalog.product_images vi ON vi.variant_id = v.id
            INNER JOIN catalog.product_image_embeddings ie ON ie.variant_image_id = vi.id
            WHERE v.is_deleted = false
              AND vi.type = 'Search'
              AND ie.model_name = {1}" +
            (excludeProductId.HasValue ? "\n              AND v.product_id != {2}" : "") + @"
            ORDER BY v.id, ie.vector <=> {0}::vector
            LIMIT {3}";

        object[] parameters;
        if (excludeProductId.HasValue)
            parameters = [queryVector, modelName, excludeProductId.Value, topK];
        else
            parameters = [queryVector, modelName, topK];

        var variants = await _dbContext.Set<Variant>()
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return variants.Select(v => v.Id).ToList();
    }

    private async Task<List<Guid>> InMemorySearchAsync(
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
                     && e.VariantImage.VariantId != null
                     && !e.VariantImage.Variant!.IsDeleted);

        if (excludeProductId.HasValue)
            query = query.Where(e => e.VariantImage.Variant!.ProductId != excludeProductId.Value);

        var embeddings = await query.ToListAsync(cancellationToken);

        return embeddings
            .GroupBy(e => e.VariantImage.VariantId!.Value)
            .Select(g => g.OrderBy(e => CosineDistance(queryArray, e.Vector!.ToArray())).First())
            .OrderBy(e => CosineDistance(queryArray, e.Vector!.ToArray()))
            .Take(topK)
            .Select(e => e.VariantImage.VariantId!.Value)
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

    public async Task<List<(Guid VariantId, double Score)>> FindSimilarWithScoresAsync(
        Vector queryVector, string modelName, int topK,
        CancellationToken cancellationToken = default)
    {
        if (_isNpgsql)
        {
            return await NpgsqlSearchWithScoresAsync(queryVector, modelName, topK, cancellationToken);
        }
        else
        {
            return await InMemorySearchWithScoresAsync(queryVector, modelName, topK, cancellationToken);
        }
    }

    private async Task<List<(Guid VariantId, double Score)>> NpgsqlSearchWithScoresAsync(
        Vector queryVector, string modelName, int topK,
        CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT DISTINCT ON (v.id) v.id AS ""VariantId"",
                   1.0 - (ie.vector <=> {0}::vector) AS ""Score""
            FROM catalog.variants v
            INNER JOIN catalog.product_images vi ON vi.variant_id = v.id
            INNER JOIN catalog.product_image_embeddings ie ON ie.variant_image_id = vi.id
            WHERE v.is_deleted = false
              AND vi.type = 'Search'
              AND ie.model_name = {1}
            ORDER BY v.id, ie.vector <=> {0}::vector
            LIMIT {2}";

        object[] parameters = [queryVector, modelName, topK];

        var dbContext = (DbContext)_dbContext;
        var results = await dbContext.Database
            .SqlQueryRaw<(Guid VariantId, double Score)>(sql, parameters)
            .ToListAsync(cancellationToken);

        return results;
    }

    private async Task<List<(Guid VariantId, double Score)>> InMemorySearchWithScoresAsync(
        Vector queryVector, string modelName, int topK,
        CancellationToken cancellationToken)
    {
        var queryArray = queryVector.ToArray();

        var query = _dbContext.Set<ImageEmbedding>()
            .Include(e => e.VariantImage)
                .ThenInclude(vi => vi.Variant)
            .Where(e => e.ModelName == modelName
                     && e.Vector != null
                     && e.VariantImage.Type == VariantImageType.Search
                     && e.VariantImage.VariantId != null
                     && !e.VariantImage.Variant!.IsDeleted);

        var embeddings = await query.ToListAsync(cancellationToken);

        return embeddings
            .GroupBy(e => e.VariantImage.VariantId!.Value)
            .Select(g => new
            {
                VariantId = g.Key,
                Best = g.OrderBy(e => CosineDistance(queryArray, e.Vector!.ToArray())).First()
            })
            .OrderBy(x => CosineDistance(queryArray, x.Best.Vector!.ToArray()))
            .Take(topK)
            .Select(x => (x.VariantId, Score: 1.0 - CosineDistance(queryArray, x.Best.Vector!.ToArray())))
            .ToList();
    }
}
