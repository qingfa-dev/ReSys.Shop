using Module.Catalog.Domain.OptionTypes;

namespace Module.Catalog.Features.Admin.OptionTypes.Delete;

/// <summary>
/// Defines the use case for deleting an existing option type.
/// </summary>
public static partial class DeleteOptionType
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Deletes an option type, preventing removal when option values exist.
        /// </summary>
        /// <param name="command">The command containing the entity ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty, post=result.IsSuccess, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch option type with associated values to check deletion eligibility
            var entity = await dbContext.Set<OptionType>()
                .Include(x => x.OptionValues)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OptionTypeResult.Failure.NotFound;

            // Enforce: Cannot delete option type with active values — FK constraint would orphan them
            if (entity.OptionValues.Count != 0)
                return OptionTypeResult.Failure.CannotDeleteWithValues;

            // Remove: Soft-delete the option type entity
            var deleteResult = entity.Delete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            dbContext.Set<OptionType>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option type deletion event for audit trail
            OptionTypeLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok(OptionTypeResult.Success.Deleted);
        }
    }
}