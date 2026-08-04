using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Update;

/// <summary>
/// Defines the use case for updating an existing option value.
/// </summary>
public static partial class UpdateOptionValue
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Updates an option value after validating parent existence and name uniqueness.
        /// </summary>
        /// <param name="command">The command containing the IDs and update payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.OptionTypeId!=Guid.Empty && command.Id!=Guid.Empty, post=result!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var optionTypeId = request.OptionTypeId;

            // Validate: Parent option type must exist to receive updates
            var optionType = await dbContext.Set<OptionType>().FindAsync([optionTypeId], cancellationToken);
            if (optionType is null)
                return OptionTypeResult.Failure.NotFound;

            // Load: Fetch the specific option value to update
            var entity = await dbContext.Set<OptionValue>()
                .Include(x => x.OptionType)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.OptionTypeId == optionTypeId, cancellationToken);

            if (entity is null)
                return OptionValueResult.Errors.NotFound;

            // Validate: Updated name must not conflict with another value in the same option type
            var nameExists = await dbContext.Set<OptionValue>()
                .AnyAsync(x => x.OptionTypeId == optionTypeId && x.Name == request.Name && x.Id != command.Id, cancellationToken);

            if (nameExists)
                return OptionValueResult.Errors.NameAlreadyExists;

            // Update: Apply incoming values to existing entity
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option value update event for audit trail
            OptionValueLoggers.Updated(logger, Name: entity.Name, Id: entity.Id, OptionTypeId: optionTypeId, ActionBy: currentUser.UserName);
            return entity.MapToListItem<Response>();
        }
    }
}