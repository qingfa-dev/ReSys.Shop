using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Admin.States.Create;

/// <summary>Handles creation of a new state.</summary>
public static partial class CreateState
{
    /// <summary>Command to create a new state.</summary>
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the create state command.</summary>
        /// <param name="command">The command containing the state request data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the created state.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
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

            // Persist: Save the new state to the database.
            dbContext.Set<State>().Add(entity: state);
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return the created state as response.
            return state.MapToListItem<Response>();
        }
    }
}
