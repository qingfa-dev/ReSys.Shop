using Hangfire;
using Hangfire.States;

using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Domain.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public sealed record Command(Request Request) : ICommand<EmbeddingDetailResponse>;

    /// <summary>Handler for creating an image embedding.</summary>
    public sealed class CommandHandler(
        IEmbeddingOrchestrator orchestrator,
        IApplicationDbContext dbContext,
        IBackgroundJobClient? backgroundJobClient,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, EmbeddingDetailResponse>
    {
        /// <summary>Creates a Pending embedding row and enqueues deferred ML processing.</summary>
        public async Task<Result<EmbeddingDetailResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            _ = orchestrator;
            _ = logger;

            var request = command.Request;
            // Resolve: Model name falls back to the default model when not supplied
            var modelName = string.IsNullOrEmpty(request.ModelName)
                ? VariantImageConstant.Defaults.DefaultEmbeddingModel
                : request.ModelName;

            // Check: Only one embedding may be pending or processing per variant image + model
            var existingPending = await dbContext.Set<ImageEmbedding>()
                .Include(e => e.VariantImage)
                .AnyAsync(e => e.VariantImageId == request.VariantImageId
                    && e.ModelName == modelName
                    && (e.Status == EmbeddingStatus.Pending || e.Status == EmbeddingStatus.Processing),
                    cancellationToken);
            if (existingPending)
                return ImageEmbeddingResult.Errors.Conflict(request.VariantImageId);

            // Create: Pending row reserving the embedding slot for deferred processing
            var embedding = ImageEmbeddingMethod.CreatePending(request.VariantImageId, modelName, "1.0");
            dbContext.Set<ImageEmbedding>().Add(embedding);

            // Persist: Save the Pending row (with its id) before enqueueing so a fast
            // worker never runs against a missing row
            await dbContext.SaveChangesAsync(cancellationToken);

            // Enqueue: Trigger ML inference as a Hangfire job correlated by embedding id
            var jobId = backgroundJobClient?.Create<IEmbeddingOrchestrator>(
                o => o.RunAsync(embedding.Id, CancellationToken.None),
                new EnqueuedState());
            embedding.HangfireJobId = jobId;

            // Persist: Save the Hangfire job id on the Pending row
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<EmbeddingDetailResponse>.Created(
                new EmbeddingDetailResponse
                {
                    Id = embedding.Id,
                    VariantImageId = embedding.VariantImageId,
                    ModelName = embedding.ModelName,
                    ModelVersion = embedding.ModelVersion,
                    Vector = [],
                    Dimensions = 0,
                    Status = embedding.Status.ToString(),
                    Error = embedding.Error,
                    HangfireJobId = embedding.HangfireJobId,
                    CompletedAtUtc = embedding.CompletedAtUtc
                },
                ImageEmbeddingResult.Success.Created(embedding.Id));
        }
    }
}
