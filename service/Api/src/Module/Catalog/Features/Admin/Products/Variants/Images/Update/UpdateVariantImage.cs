using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Update;

/// <summary>
/// Defines the use case for updating a variant image.
/// </summary>
public static partial class UpdateVariantImage
{
    public sealed record Command(Guid ImageId, Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles updating variant image metadata: alt text, display position, and type classification.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Executes the update: loads image, applies metadata changes, persists.
        /// </summary>
        /// <param name="command">The command containing image ID and update payload.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>The updated image detail, or a not-found failure.</returns>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var (imageId, request) = command;

            // Load: Fetch existing image entity from database
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(x => x.Id == imageId, cancellationToken);

            // Check: Return 404 if image does not exist
            if (image is null)
                return VariantImageResult.Failure.ById(imageId);

            // Parse: Convert request type string to domain enum, preserve existing if not provided
            var imageType = !string.IsNullOrEmpty(request.Type)
                && Enum.TryParse<VariantImageType>(request.Type, ignoreCase: true, out var parsedType)
                ? parsedType
                : image.Type;

            // Update: Apply alt text, position, and type changes to the entity
            var updateResult = image.UpdateDetails(
                position: request.Position,
                alt: request.Alt,
                type: imageType);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record image update event for observability
            VariantImageLoggers.Updated(logger, Id: image.Id, ActionBy: currentUser.UserName);

            // Map: Return updated image as detail DTO
            return Result<Response>.Ok(image.MapToDetail<Response>());
        }
    }
}
