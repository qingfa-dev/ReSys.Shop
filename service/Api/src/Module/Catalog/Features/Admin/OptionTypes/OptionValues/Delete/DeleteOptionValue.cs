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
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Assign: Extract option type ID for cleaner logic
            var optionTypeId = command.OptionTypeId;

            // Check: Ensure the parent option type exists in the database
            var typeExists = await dbContext.Set<OptionType>().AnyAsync(x => x.Id == optionTypeId, cancellationToken);
            if (!typeExists)
                return OptionTypeResult.Failure.NotFound;

            // Check: Find the specific option value entity to delete
            var entity = await dbContext.Set<OptionValue>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.OptionTypeId == optionTypeId, cancellationToken);

            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            // Remove: Mark the entity for deletion and persist changes
            dbContext.Set<OptionValue>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record successful deletion with identity context
            OptionValueLoggers.Deleted(logger, Name: entity.Name, Id: entity.Id, OptionTypeId: entity.OptionTypeId, ActionBy: currentUser.UserName);

            // Create: Return response containing the ID of the deleted resource
            return new Response { Id = entity.Id };
        }
    }
}
