using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public sealed record Command(Guid VariantImageId, string ModelName) : ICommand<EmbeddingDetailResponse>;

    public sealed class CommandHandler(IEmbeddingOrchestrator orchestrator)
        : ICommandHandler<Command, EmbeddingDetailResponse>
    {
        public async Task<Result<EmbeddingDetailResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            var result = await orchestrator.GenerateAndPersistAsync(command.VariantImageId, command.ModelName, cancellationToken);
            if (result.IsFailure)
                return result.Errors;

            return Result<EmbeddingDetailResponse>.Created(result.Value, ImageEmbeddingResult.Success.Created(result.Value.Id));
        }
    }
}
