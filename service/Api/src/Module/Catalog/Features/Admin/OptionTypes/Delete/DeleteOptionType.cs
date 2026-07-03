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
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Find the existing entity and include its values to check for association.
            var entity = await dbContext.Set<OptionType>()
                .Include(x => x.OptionValues)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OptionTypeResult.Failure.NotFound;

            // Check: Prevent deletion if there are associated option values.
            if (entity.OptionValues.Count != 0)
                return OptionTypeResult.Failure.CannotDeleteWithValues;

            // Delete: Remove the entity from the database.
            var deleteResult = entity.Delete();
            if (deleteResult.IsFailure)
                return deleteResult.Errors;

            // Persist: Save changes to the database.
            dbContext.Set<OptionType>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option type deletion event for audit trail
            OptionTypeLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok(OptionTypeResult.Success.Deleted);
        }
    }
}
