using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

using Shared.Operational.Persistence.Data;

using Microsoft.EntityFrameworkCore;

namespace Module.Location.Features.Admin.Countries.Update;

/// <summary>Handles update of an existing country.</summary>
public static partial class UpdateCountry
{
    // ============ COMMAND ============
    /// <summary>Command to update an existing country.</summary>
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    // ============ COMMAND HANDLER ============
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the update country command.</summary>
        /// <param name="command">The command containing the update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the updated country details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Check: Country exists for the given Id.
            var country = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == command.Id, cancellationToken: cancellationToken);

            if (country is null)
                return CountryResult.Errors.NotFound;

            // Validate: No duplicate ISO code with another country (case-insensitive).
            var duplicateCountry = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c =>
                    c.IsoCode == request.IsoCode &&
                    c.Id != command.Id, cancellationToken: cancellationToken);

            if (duplicateCountry is not null)
                return CountryResult.Errors.IsoCodeDuplicate;

            // Update: Apply request data to country entity.
            request.MapToDomain(country: country);

            // Persist: Save changes to database.
            dbContext.Set<Country>().Update(entity: country);
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return updated country as response.
            return country.MapToDetail<Response>();
        }
    }
}