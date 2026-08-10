using Module.Catalog.Domain.Variants.Images;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Admin.Variants.Images.Delete;

/// <summary>
/// Defines the use case for deleting a variant image.
/// </summary>
public static partial class DeleteVariantImage
{
    public sealed record Command(Guid ImageId) : ICommand<Response>;

    /// <summary>
    /// Handles deleting a variant image: removes from storage, then deletes the entity.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStorageService storageService,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Executes the delete: loads image, removes storage file, deletes entity, persists.
        /// </summary>
        /// <param name="command">The command containing the image ID to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A success response with confirmation message, or a failure result.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch existing image entity from database
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(x => x.Id == command.ImageId, cancellationToken);

            // Check: Return 404 if image does not exist
            if (image is null)
                return VariantImageResult.Failure.ById(command.ImageId);

            // Call: Remove the physical file from storage provider
            var removeResult = await storageService.DeleteAsync(image.StoragePath, ct: cancellationToken);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            // Remove: Delete the entity from the data context (permanent, not soft-delete)
            dbContext.Set<VariantImage>().Remove(image);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record image deletion event for observability
            VariantImageLoggers.Deleted(logger, Id: image.Id, ActionBy: currentUser.UserName);

            return Result<Response>.Ok(new Response { Message = VariantImageResult.Success.Deleted(image.Id) });
        }
    }
}