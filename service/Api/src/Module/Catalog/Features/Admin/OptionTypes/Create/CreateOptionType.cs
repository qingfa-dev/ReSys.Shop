using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.Create;

/// <summary>
/// Defines the use case for creating a new option type.
/// </summary>
public static partial class CreateOptionType
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
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
            var request = command.Request;

            // Check: Verify if an option type with the same name already exists.
            var nameExists = await dbContext.Set<OptionType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (nameExists)
                return OptionTypeResult.Failure.DuplicateName;

            // Create: Map the request to a new OptionType entity.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            // Persist: Add entity to database and save changes.
            dbContext.Set<OptionType>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return created response
            return Result<Response>.Created(
                entity.MapToDetail<Response>(),
                 OptionTypeResult.Success.Created);
        }
    }
}
