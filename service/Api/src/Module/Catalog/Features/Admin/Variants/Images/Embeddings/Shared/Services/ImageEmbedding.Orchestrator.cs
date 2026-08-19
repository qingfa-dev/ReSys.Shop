using Microsoft.Extensions.Options;

using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

/// <summary>Orchestrates image embedding generation via the inference service and persisting results to the database.</summary>
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

    /// <summary>Generates an embedding for a variant image URL and persists it to the database.</summary>
    /// <param name="variantImageId">The variant image identifier.</param>
    /// <param name="modelName">The embedding model name (uses default if empty).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the persisted embedding details.</returns>
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

    /// <summary>Generates an embedding from raw image bytes and persists it to the database.</summary>
    /// <param name="variantImageId">The variant image identifier.</param>
    /// <param name="imageBytes">Raw image byte data.</param>
    /// <param name="contentType">MIME content type of the image.</param>
    /// <param name="modelName">The embedding model name (uses default if empty).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the persisted embedding details.</returns>
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
        var imageStillExists = await _dbContext.Set<VariantImage>()
            .AnyAsync(x => x.Id == variantImageId, ct);
        if (!imageStillExists)
        {
            Loggers.VariantImageDeletedDuringEmbedding(_logger, variantImageId, modelName);
            return ImageEmbeddingResult.Errors.VariantImageDeletedDuringEmbedding(variantImageId);
        }

        var existing = await _dbContext.Set<ImageEmbedding>()
            .Include(e => e.VariantImage)
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

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            Loggers.VariantImageDeletedDuringEmbedding(_logger, variantImageId, modelName);
            return ImageEmbeddingResult.Errors.VariantImageDeletedDuringEmbedding(variantImageId);
        }

        Loggers.EmbeddingPersisted(_logger, variantImageId, modelName, embedding.Id);

        return Result<EmbeddingDetailResponse>.Ok(new EmbeddingDetailResponse
        {
            Id = embedding.Id,
            VariantImageId = embedding.VariantImageId,
            ModelName = embedding.ModelName,
            ModelVersion = embedding.ModelVersion,
            Vector = embedding.Vector!.ToArray(),
            Dimensions = embedding.Dimensions
        });
    }

    /// <summary>Runs the embedding generation job for an embedding in the Pending status, persisting the result or failure.</summary>
    /// <param name="embeddingId">The image embedding identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating whether the job ran; failures are recorded on the embedding entity.</returns>
    public async Task<Result> RunAsync(Guid embeddingId, CancellationToken ct = default)
    {
        var embedding = await _dbContext.Set<ImageEmbedding>()
            .Include(e => e.VariantImage)
            .FirstOrDefaultAsync(e => e.Id == embeddingId, ct);
        if (embedding is null)
        {
            Loggers.RunFailed(_logger, embeddingId, "Embedding not found");
            return ImageEmbeddingResult.Errors.NotFound(embeddingId);
        }

        Loggers.RunStarted(_logger, embeddingId, embedding.VariantImageId, embedding.ModelName);

        var markResult = ImageEmbeddingMethod.MarkProcessing(embedding);
        if (markResult.IsFailure)
        {
            Loggers.RunFailed(_logger, embeddingId,
                $"Status transition failed: {markResult.Errors.FirstOrDefault().Message ?? "Unknown error"}");
            return markResult.Errors;
        }
        await _dbContext.SaveChangesAsync(ct);

        Loggers.RunProcessing(_logger, embeddingId);

        var image = await _dbContext.Set<VariantImage>()
            .FirstOrDefaultAsync(x => x.Id == embedding.VariantImageId, ct);
        if (image is null)
        {
            ImageEmbeddingMethod.MarkFailed(embedding, "Image was deleted");
            await _dbContext.SaveChangesAsync(ct);
            Loggers.RunFailed(_logger, embeddingId, "Image was deleted");
            return Result.Ok();
        }

        if (string.IsNullOrEmpty(image.Url))
        {
            ImageEmbeddingMethod.MarkFailed(embedding, "VariantImage has no public URL");
            await _dbContext.SaveChangesAsync(ct);
            Loggers.RunFailed(_logger, embeddingId, "No public URL");
            return Result.Ok();
        }

        var request = new EmbeddingRequest { ImageUrl = image.Url, Model = embedding.ModelName };
        var inferenceResult = await _inferenceClient.CreateEmbeddingAsync(request, ct);
        if (inferenceResult.IsFailure)
        {
            var errorMsg = inferenceResult.Errors.FirstOrDefault().Message ?? "Unknown inference error";
            ImageEmbeddingMethod.MarkFailed(embedding, errorMsg);
            await _dbContext.SaveChangesAsync(ct);
            Loggers.RunFailed(_logger, embeddingId, errorMsg);
            return Result.Ok();
        }

        var inference = inferenceResult.Value;
        var completeResult = ImageEmbeddingMethod.MarkCompleted(
            embedding, inference.Vector.ToArray(), inference.Dimension, inference.ModelVersion);
        if (completeResult.IsFailure)
        {
            Loggers.RunFailed(_logger, embeddingId,
                $"MarkCompleted failed: {completeResult.Errors.FirstOrDefault().Message ?? "Unknown error"}");
            return completeResult.Errors;
        }
        await _dbContext.SaveChangesAsync(ct);

        Loggers.RunCompleted(_logger, embeddingId, inference.Dimension);
        return Result.Ok();
    }

    private static bool IsForeignKeyViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException postgresEx
            && postgresEx.SqlState == "23503";
    }
}