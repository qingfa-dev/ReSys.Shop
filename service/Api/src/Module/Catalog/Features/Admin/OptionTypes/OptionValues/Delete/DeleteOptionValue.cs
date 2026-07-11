using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Delete;

/// <summary>
/// Defines the use case for deleting an existing option value.
/// </summary>
public static partial class DeleteOptionValue
{
    public sealed record Command(Guid OptionTypeId, Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Deletes an option value after confirming the parent option type exists.
        /// </summary>
        /// <param name="command">The command containing the parent type ID and value ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.OptionTypeId!=Guid.Empty && command.Id!=Guid.Empty, post=result.Id!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var optionTypeId = command.OptionTypeId;

            // Validate: Parent option type must exist before deleting its child value
            var typeExists = await dbContext.Set<OptionType>().AnyAsync(x => x.Id == optionTypeId, cancellationToken);
            if (!typeExists)
                return OptionTypeResult.Failure.NotFound;

            // Load: Fetch the specific option value to delete
            var entity = await dbContext.Set<OptionValue>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.OptionTypeId == optionTypeId, cancellationToken);

            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            // Remove: Delete the option value entity
            dbContext.Set<OptionValue>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record successful deletion with identity context
            OptionValueLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, OptionTypeId: entity.OptionTypeId, ActionBy: currentUser.UserName);

            // Map: Return deleted resource ID in response
            return new Response { Id = entity.Id };
        }
    }
}
