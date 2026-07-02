using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

using Shared.Operational.Persistence.Data;

using Microsoft.EntityFrameworkCore;

namespace Module.Location.Features.Admin.Countries.Delete;

/// <summary>Handles deletion of a country.</summary>
public static partial class DeleteCountry
{
    // ============ COMMAND ============
    /// <summary>Command to delete a country.</summary>
    public sealed record Command(Guid Id) : ICommand<Response>;

    // ============ COMMAND HANDLER ============
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the delete country command.</summary>
        /// <param name="command">The command containing the country identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the deleted country details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Find the country by identifier.
            var country = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == command.Id, cancellationToken: cancellationToken);

            if (country is null)
                return CountryResult.Errors.NotFound;

            // Validate: Cannot delete country with associated states.
            var hasStates = await dbContext.Set<State>()
                .AnyAsync(predicate: s => s.CountryId == command.Id, cancellationToken: cancellationToken);

            if (hasStates)
                return CountryResult.Errors.CannotDeleteWithStates;

            // Remove: Delete the country from the database.
            dbContext.Set<Country>().Remove(entity: country);

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return the deleted country as response.
            return country.MapToListItem<Response>();
        }
    }
}