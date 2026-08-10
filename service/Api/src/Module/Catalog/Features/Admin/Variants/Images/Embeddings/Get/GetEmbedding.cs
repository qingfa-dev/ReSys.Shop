using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Get;

public static partial class GetEmbedding
{
    public sealed record Query(Guid VariantImageId) : IQuery<EmbeddingDetailResponse>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, EmbeddingDetailResponse>
    {
        public async Task<Result<EmbeddingDetailResponse>> Handle(
            Query query, CancellationToken cancellationToken)
        {
            var embedding = await dbContext.Set<ImageEmbedding>()
                .FirstOrDefaultAsync(e => e.VariantImageId == query.VariantImageId, cancellationToken);

            if (embedding is null)
                return ImageEmbeddingResult.Errors.NotFoundByVariantImage(query.VariantImageId);

            return Result<EmbeddingDetailResponse>.Ok(new EmbeddingDetailResponse
            {
                Id = embedding.Id,
                VariantImageId = embedding.VariantImageId,
                ModelName = embedding.ModelName,
                ModelVersion = embedding.ModelVersion,
                Vector = embedding.Vector?.ToArray() ?? [],
                Dimensions = embedding.Dimensions,
                Status = embedding.Status.ToString(),
                Error = embedding.Error,
                HangfireJobId = embedding.HangfireJobId,
                CompletedAtUtc = embedding.CompletedAtUtc
            });
        }
    }
}
