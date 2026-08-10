using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Variants.Images.Embeddings;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    public sealed record Command(Guid VariantImageId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command command, CancellationToken cancellationToken)
        {
            var embedding = await dbContext.Set<ImageEmbedding>()
                .Include(e => e.VariantImage)
                .FirstOrDefaultAsync(e => e.VariantImageId == command.VariantImageId, cancellationToken);

            if (embedding is null)
                return ImageEmbeddingResult.Errors.NotFoundByVariantImage(command.VariantImageId);

            dbContext.Set<ImageEmbedding>().Remove(embedding);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Ok(new Response { Message = ImageEmbeddingResult.Success.Deleted(embedding.Id) });
        }
    }
}
