using Microsoft.Extensions.Options;

using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

public sealed partial class EmbeddingOrchestrator : IEmbeddingOrchestrator
{
    private readonly IInferenceClient _inferenceClient;
    private readonly IApplicationDbContext _dbContext;
    private readonly EmbeddingOrchestratorOptions _options;
    private readonly ILogger<EmbeddingOrchestrator> _logger;

    public EmbeddingOrchestrator(
        IInferenceClient inferenceClient,
        IApplicationDbContext dbContext,
        IOptions<EmbeddingOrchestratorOptions> options,
        ILogger<EmbeddingOrchestrator> logger)
    {
        _inferenceClient = inferenceClient;
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<EmbeddingDetailResponse>> GenerateAndPersistAsync(Guid variantImageId, string modelName, CancellationToken ct = default)
    {
        var imageResult = await LoadVariantImageAsync(variantImageId, ct);
        if (imageResult.IsFailure)
            return imageResult.Errors;

        var image = imageResult.Value;
        var effectiveModel = string.IsNullOrEmpty(modelName) ? _options.DefaultModel : modelName;

        if (string.IsNullOrEmpty(image.Url))
            return ImageEmbeddingResult.Errors.CommunicationFailed("VariantImage has no public URL.");

        var request = new EmbeddingRequest { ImageUrl = image.Url, Model = effectiveModel };
        var inferenceResult = await _inferenceClient.CreateEmbeddingAsync(request, ct);
        if (inferenceResult.IsFailure)
            return inferenceResult.Errors;

        return await PersistEmbeddingAsync(variantImageId, effectiveModel, inferenceResult.Value, ct);
    }

    public async Task<Result<EmbeddingDetailResponse>> GenerateAndPersistFromBytesAsync(Guid variantImageId, byte[] imageBytes, string contentType, string modelName, CancellationToken ct = default)
    {
        var imageResult = await LoadVariantImageAsync(variantImageId, ct);
        if (imageResult.IsFailure)
            return imageResult.Errors;

        var effectiveModel = string.IsNullOrEmpty(modelName) ? _options.DefaultModel : modelName;

        var inferenceResult = await _inferenceClient.CreateEmbeddingFromBytesAsync(imageBytes, contentType, effectiveModel, ct);
        if (inferenceResult.IsFailure)
            return inferenceResult.Errors;

        return await PersistEmbeddingAsync(variantImageId, effectiveModel, inferenceResult.Value, ct);
    }

    private async Task<Result<VariantImage>> LoadVariantImageAsync(Guid variantImageId, CancellationToken ct)
    {
        var image = await _dbContext.Set<VariantImage>()
            .FirstOrDefaultAsync(x => x.Id == variantImageId, ct);

        if (image is null)
            return ImageEmbeddingResult.Errors.NotFound(variantImageId);

        return Result<VariantImage>.Ok(image);
    }

    private async Task<Result<EmbeddingDetailResponse>> PersistEmbeddingAsync(Guid variantImageId, string modelName, EmbeddingResponse inferenceResult, CancellationToken ct)
    {
        var existing = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId && e.ModelName == modelName, ct);

        ImageEmbedding embedding;
        if (existing is not null)
        {
            existing.ModelVersion = inferenceResult.ModelVersion;
            existing.Vector = new Pgvector.Vector(inferenceResult.Vector.ToArray());
            existing.Dimensions = inferenceResult.Dimension;
            embedding = existing;
        }
        else
        {
            embedding = ImageEmbeddingMethod.Create(
                variantImageId,
                modelName,
                inferenceResult.ModelVersion,
                inferenceResult.Vector.ToArray());
            _dbContext.Set<ImageEmbedding>().Add(embedding);
        }

        await _dbContext.SaveChangesAsync(ct);

        Loggers.EmbeddingPersisted(_logger, variantImageId, modelName, embedding.Id);

        return Result<EmbeddingDetailResponse>.Ok(new EmbeddingDetailResponse
        {
            Id = embedding.Id,
            VariantImageId = embedding.VariantImageId,
            ModelName = embedding.ModelName,
            ModelVersion = embedding.ModelVersion,
            Vector = embedding.Vector.ToArray(),
            Dimensions = embedding.Dimensions
        });
    }
}
