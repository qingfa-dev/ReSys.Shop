using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Update;

/// <summary>
/// Defines the use case for updating an existing option value.
/// </summary>
public static partial class UpdateOptionValue
{
    public sealed record Command(Guid OptionTypeId, Guid Id, Request Request) : ICommand<Response>;

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
            var optionTypeId = command.OptionTypeId;
            var request = command.Request;

            // Check: Ensure parent option type exists
            var optionType = await dbContext.Set<OptionType>().FindAsync([optionTypeId], cancellationToken);
            if (optionType is null)
                return OptionTypeResult.Failure.NotFound;

            // Check: Find the specific option value entity to update
            var entity = await dbContext.Set<OptionValue>()
                .Include(x => x.OptionType)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.OptionTypeId == optionTypeId, cancellationToken);

            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            // Check: Verify uniqueness of the new name within the same option type (excluding the entity being updated)
            var nameExists = await dbContext.Set<OptionValue>()
                .AnyAsync(x => x.OptionTypeId == optionTypeId && x.Name == request.Name && x.Id != command.Id, cancellationToken);

            if (nameExists)
                return OptionValueResult.Errors.NameAlreadyExists;

            // Update: Apply changes from request to the domain entity
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option value update event for audit trail
            OptionValueLoggers.Updated(logger, Name: entity.Name, Id: entity.Id, OptionTypeId: optionTypeId, ActionBy: currentUser.UserName);
            return entity.MapToListItem<Response>();
        }
    }
}
