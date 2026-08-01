using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Create;

/// <summary>
/// Defines the use case for creating a new option value.
/// </summary>
public static partial class CreateOptionValue
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Creates a new option value under a parent option type.
        /// </summary>
        /// <param name="command">The command containing the parent ID and value payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.OptionTypeId!=Guid.Empty, post=result.Id!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var optionTypeId = request.OptionTypeId;

            // Validate: Parent option type must exist to receive new values
            var optionType = await dbContext.Set<OptionType>().FindAsync([optionTypeId], cancellationToken);
            if (optionType is null)
                return OptionTypeResult.Failure.NotFound;

            // Validate: Value name must be unique within the parent option type
            var nameExists = await dbContext.Set<OptionValue>()
                .AnyAsync(x => x.OptionTypeId == optionTypeId && x.Name == request.Name, cancellationToken);

            if (nameExists)
                return OptionValueResult.Errors.NameAlreadyExists;

            // Create: Instantiate new OptionValue entity from validated request
            var result = request.MapToDomain(optionTypeId);
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

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