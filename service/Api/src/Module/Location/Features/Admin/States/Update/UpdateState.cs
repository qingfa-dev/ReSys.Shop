using Module.Location.Domain.Countries;
using Module.Location.Domain.States;
using Module.Location.Features.Shared.States.Mappings;

namespace Module.Location.Features.Admin.States.Update;

/// <summary>Updates an existing state with abbreviation uniqueness validation.</summary>
public static partial class UpdateState
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates state existence, country reference, and abbreviation uniqueness, then applies updates.</summary>
        /// <param name="command">The command containing the update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated state details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=state!=null && country exists && abbreviation unique, post=state updated,
            //           throws=DbUpdateException
            var request = command.Request;

            // Check: Find the existing state.
            var entity = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Id == command.Id, cancellationToken: cancellationToken);

            if (entity is null)
                return StateResult.Failure.NotFound;

            // Check: Verify the referenced country exists.
            var existingCountry = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == request.CountryId, cancellationToken: cancellationToken);

            if (existingCountry is null)
                return StateResult.Failure.CountryNotFound;

            // Validate: No duplicate state abbreviation for the same country.
            var duplicateState = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s =>
                    s.Abbreviation == request.Abbreviation &&
                    s.CountryId == request.CountryId &&
                    s.Id != command.Id, cancellationToken: cancellationToken);

            if (duplicateState is not null)
                return StateResult.Failure.AbbreviationDuplicate;

            // Update: Apply request data to state entity.
            request.MapToDomain(state: entity);

            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
            // Map: Return the updated state as response.
            return entity.MapToDetail<Response>();
        }
    }
}