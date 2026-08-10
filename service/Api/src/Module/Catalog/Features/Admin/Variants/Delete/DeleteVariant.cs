using Module.Catalog.Domain.Variants;

namespace Module.Catalog.Features.Admin.Variants.Delete;

/// <summary>
/// Defines the use case for deleting (soft-deleting) a variant.
/// </summary>
public static partial class DeleteVariant
{
    public sealed record Command(Guid Id) : ICommand;

    /// <summary>
    /// Soft-deletes a variant. Guards against double-deletion and
    /// persists the soft-delete state.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Handles the delete-variant command — loads the variant, validates
        /// it is not already deleted, soft-deletes via domain method, and persists.
        /// </summary>
        /// <param name="command">The command containing the variant ID to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A deleted result with the variant ID.</returns>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=entity.IsDeleted==true, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch variant by ID for soft-delete
            var entity = await dbContext.Set<Variant>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return VariantResult.Errors.NotFound(command.Id);

            // Check: Prevent double-deletion — variant already soft-deleted
            if (entity.IsDeleted)
                return VariantResult.Errors.AlreadyDeleted;

            // Remove: Soft-delete variant via domain method
            var deleteResult = entity.Delete(currentUser.UserName ?? "System");
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            dbContext.Set<Variant>().Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record variant deletion event for audit trail
            VariantLoggers.Deleted(logger, Sku: entity.Sku!, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.NoContent(VariantResult.Success.Deleted(entity.Id));
        }
    }
}