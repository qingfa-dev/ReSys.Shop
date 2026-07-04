using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Create;

/// <summary>
/// Defines the use case for creating a new option value.
/// </summary>
public static partial class CreateOptionValue
{
    public sealed record Command(Guid OptionTypeId, Request Request) : ICommand<Response>;

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

            // Check: Ensure parent option type exists in the database
            var optionType = await dbContext.Set<OptionType>().FindAsync([optionTypeId], cancellationToken);
            if (optionType is null)
                return OptionTypeResult.Failure.NotFound;

            // Check: Verify uniqueness of the name within the same option type
            var nameExists = await dbContext.Set<OptionValue>()
                .AnyAsync(x => x.OptionTypeId == optionTypeId && x.Name == request.Name, cancellationToken);

            if (nameExists)
                return OptionValueResult.Errors.NameAlreadyExists;

            // Create: Instantiate new OptionValue entity from request data
            var result = request.MapToDomain(optionTypeId);
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            // Persist: Add entity to database and save changes.
            dbContext.Set<OptionValue>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option value creation event for audit trail
            OptionValueLoggers.Created(logger, Name: entity.Name, Id: entity.Id, OptionTypeId: optionTypeId, ActionBy: currentUser.UserName);
            return Result<Response>.Created(
                entity.MapToListItem<Response>(),
                OptionValueResult.Success.Created(entity.Id));
        }
    }
}
