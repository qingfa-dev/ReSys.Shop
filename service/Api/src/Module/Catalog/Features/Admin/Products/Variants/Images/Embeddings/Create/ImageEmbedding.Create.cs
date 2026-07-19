using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public sealed record Command(Request Request) : ICommand<EmbeddingDetailResponse>;

    /// <summary>Handler for creating an image embedding.</summary>
    public sealed class CommandHandler(IEmbeddingOrchestrator orchestrator)
        : ICommandHandler<Command, EmbeddingDetailResponse>
    {
        /// <summary>Creates an image embedding.</summary>
        public async Task<Result<EmbeddingDetailResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var result = await orchestrator.GenerateAndPersistAsync(request.VariantImageId, request.ModelName, cancellationToken);
            if (result.IsFailure)
                return result.Errors;

            return Result<EmbeddingDetailResponse>.Created(result.Value, ImageEmbeddingResult.Success.Created(result.Value.Id));
        }
    }
}