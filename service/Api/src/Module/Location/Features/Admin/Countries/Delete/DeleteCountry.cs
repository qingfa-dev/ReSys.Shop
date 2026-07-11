using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

namespace Module.Location.Features.Admin.Countries.Delete;

/// <summary>Deletes a country after validating it has no associated states.</summary>
public static partial class DeleteCountry
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates no states reference the country, then hard-deletes it.</summary>
        /// <param name="command">The command containing the country identifier.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the deleted country details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=country!=null && no associated states, post=country removed, throws=DbUpdateException
            // Load: Find the country by identifier.
            var country = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == command.Id, cancellationToken: cancellationToken);

            if (country is null)
                return CountryResult.Failure.NotFound;

            // Validate: Cannot delete country with associated states.
            var hasStates = await dbContext.Set<State>()
                .AnyAsync(predicate: s => s.CountryId == command.Id, cancellationToken: cancellationToken);

            if (hasStates)
                return CountryResult.Failure.CannotDeleteWithStates;

            // Remove: Delete the country from the database.
            dbContext.Set<Country>().Remove(entity: country);

            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return the deleted country as response.
            return country.MapToListItem<Response>();
        }
    }
}