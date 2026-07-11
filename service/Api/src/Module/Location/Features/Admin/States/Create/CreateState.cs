using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Admin.States.Create;

/// <summary>Creates a new state for an existing country with abbreviation uniqueness validation.</summary>
public static partial class CreateState
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates the country exists and abbreviation is unique, then persists the new state.</summary>
        /// <param name="command">The command containing the state request data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created state or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=country exists && abbreviation unique for country, post=state persisted,
            //           throws=DbUpdateException
            var request = command.Request;

            // Check: Verify the referenced country exists.
            var existingCountry = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == request.CountryId, cancellationToken: cancellationToken);

            if (existingCountry is null)
                return StateResult.Failure.CountryNotFound;

            // Validate: No duplicate state abbreviation for the same country.
            var existingState = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Abbreviation == request.Abbreviation
                                                     && s.CountryId == request.CountryId, cancellationToken: cancellationToken);

            if (existingState is not null)
                return StateResult.Failure.AbbreviationDuplicate;

            // Create: Map request to domain entity.
            var state = request.MapToDomain();

            dbContext.Set<State>().Add(entity: state);
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return the created state as response.
            return Result<Response>.Created(state.MapToListItem<Response>());
        }
    }
}
