using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Delete;

/// <summary>
/// Defines the use case for deleting an existing option value.
/// </summary>
public static partial class DeleteOptionValue
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        /// <summary>
        /// Deletes an option value after confirming the parent option type exists.
        /// </summary>
        /// <param name="command">The command containing the parent type ID and value ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.OptionTypeId!=Guid.Empty && command.Id!=Guid.Empty, post=result!=null, throws=DbUpdateException
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch the specific option value to delete
            var entity = await dbContext.Set<OptionValue>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            var optionTypeId = entity.OptionTypeId;

            // Remove: Delete the option value entity
            dbContext.Set<OptionValue>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record successful deletion with identity context
            OptionValueLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, OptionTypeId: entity.OptionTypeId, ActionBy: currentUser.UserName);

            // Map: Return deleted resource ID in response
            return Result.Ok(OptionValueResult.Success.Deleted(entity.Id));
        }
    }
}