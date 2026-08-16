using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.Shared.Mappings;

namespace Module.Catalog.Features.Admin.OptionTypes.Update;

/// <summary>
/// Defines the use case for updating an existing option type.
/// </summary>
public static partial class UpdateOptionType
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Updates an existing option type after validating name uniqueness.
        /// </summary>
        /// <param name="command">The command containing the ID and update payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="DbUpdateException">Thrown when persistence fails.</exception>
        // Contract: pre=command.Id!=Guid.Empty && command.Request!=null, post=result!=null, throws=DbUpdateException
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load: Fetch the existing option type to modify
            var entity = await dbContext.Set<OptionType>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OptionTypeResult.Failure.NotFound;

            // Validate: Updated name must not conflict with another option type
            var nameExists = await dbContext.Set<OptionType>()
                .AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken);

            if (nameExists)
                return OptionTypeResult.Failure.DuplicateName;

            // Update: Apply incoming request values to existing entity
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option type update event for audit trail
            // Map: Return updated response
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                OptionTypeResult.Success.Updated);
        }
    }
}