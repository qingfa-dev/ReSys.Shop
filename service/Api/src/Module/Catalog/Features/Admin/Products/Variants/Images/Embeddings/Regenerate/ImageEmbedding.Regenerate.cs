using Hangfire;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

public static partial class RegenerateEmbedding
{
    public sealed record Command(Request Request) : ICommand<EmbeddingDetailResponse>;

    /// <summary>Handler for regenerating an image embedding.</summary>
    public sealed class CommandHandler(
        IEmbeddingOrchestrator orchestrator,
        IApplicationDbContext dbContext,
        IBackgroundJobClient? backgroundJobClient,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, EmbeddingDetailResponse>
    {
        /// <summary>Resets the embedding to Pending (or creates it if absent) and enqueues deferred ML processing.</summary>
        public async Task<Result<EmbeddingDetailResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            _ = orchestrator;
            _ = logger;

            var request = command.Request;
            // Resolve: Model name falls back to the default model when not supplied
            var modelName = string.IsNullOrEmpty(request.ModelName)
                ? VariantImageConstant.Defaults.DefaultEmbeddingModel
                : request.ModelName;
            var modelVersion = request.ModelVersion ?? "1.0";

            var existing = await dbContext.Set<ImageEmbedding>()
                .FirstOrDefaultAsync(e => e.VariantImageId == request.VariantImageId
                    && e.ModelName == modelName, cancellationToken);

            ImageEmbedding embedding;
            if (existing is null)
            {
                // Create: Missing row means it was deleted; reserve a fresh Pending slot
                embedding = ImageEmbeddingMethod.CreatePending(request.VariantImageId, modelName, modelVersion);
                dbContext.Set<ImageEmbedding>().Add(embedding);
            }
            else
            {
                // Reset: Requeue the existing embedding, clearing prior completion state
                var pendingResult = ImageEmbeddingMethod.MarkPending(existing);
                if (pendingResult.IsFailure)
                    return pendingResult.Errors;
                embedding = existing;
            }

            // Persist: Save the Pending row (with its id) before enqueueing so a fast
            // worker never runs against a missing row
            await dbContext.SaveChangesAsync(cancellationToken);

            // Enqueue: Trigger ML inference as a Hangfire job correlated by embedding id
            var jobId = backgroundJobClient?.Create<IEmbeddingOrchestrator>(
                o => o.RunAsync(embedding.Id, CancellationToken.None),
                new EnqueuedState());
            embedding.HangfireJobId = jobId;

            // Persist: Save the Hangfire job id on the embedding row
            await dbContext.SaveChangesAsync(cancellationToken);

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