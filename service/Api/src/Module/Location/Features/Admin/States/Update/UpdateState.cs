using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

using Shared.Operational.Persistence.Data;

using Microsoft.EntityFrameworkCore;

namespace Module.Location.Features.Admin.States.Update;

/// <summary>Handles update of an existing state.</summary>
public static partial class UpdateState
{
    /// <summary>Command to update an existing state.</summary>
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the update state command.</summary>
        /// <param name="command">The command containing the update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the updated state details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Find the existing state.
            var entity = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Id == command.Id, cancellationToken: cancellationToken);

            if (entity is null)
                return StateResult.Errors.NotFound;

            // Check: Verify the referenced country exists.
            var existingCountry = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == request.CountryId, cancellationToken: cancellationToken);

            if (existingCountry is null)
                return StateResult.Errors.CountryNotFound;

            // Validate: No duplicate state abbreviation for the same country.
            var duplicateState = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s =>
                    s.Abbreviation == request.Abbreviation &&
                    s.CountryId == request.CountryId &&
                    s.Id != command.Id, cancellationToken: cancellationToken);

            if (duplicateState is not null)
                return StateResult.Errors.AbbreviationDuplicate;

            // Update: Apply request data to state entity.
            request.MapToDomain(state: entity);

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
            // Map: Return the updated state as response.
            return entity.MapToDetail<Response>();
        }
    }
}