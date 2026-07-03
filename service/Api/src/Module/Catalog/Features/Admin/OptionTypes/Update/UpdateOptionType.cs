using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings;

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
        /// Handles the request and returns a result.
        /// </summary>
        /// <param name="command">The command containing request data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Query: Fetch the existing option type by ID.
            var entity = await dbContext.Set<OptionType>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OptionTypeResult.Failure.NotFound;

            // Check: Verify if an option type with the same name already exists (excluding the current one).
            var nameExists = await dbContext.Set<OptionType>()
                .AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken);

            if (nameExists)
                return OptionTypeResult.Failure.DuplicateName;

            // Update: Apply changes from the request to the entity.
            var result = request.MapToDomain(entity);
            if (result.IsFailure)
                return result.Errors;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record option type update event for audit trail
            // Map: Return updated response
            return Result<Response>.Ok(
                entity.MapToDetail<Response>(),
                OptionTypeResult.Success.Updated);
        }
    }
}
