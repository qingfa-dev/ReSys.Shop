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
        /// Creates a new option type after validating name uniqueness.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Validate: Option type name must be unique within the system
            var nameExists = await dbContext.Set<OptionType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (nameExists)
                return OptionTypeResult.Failure.DuplicateName;

            // Create: Instantiate OptionType entity from validated request
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Errors;

            var entity = result.Value;

            dbContext.Set<OptionType>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return created response
            return Result<Response>.Created(
                entity.MapToDetail<Response>(),
                 OptionTypeResult.Success.Created);
        }
    }
}