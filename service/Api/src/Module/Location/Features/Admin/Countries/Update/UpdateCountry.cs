using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

namespace Module.Location.Features.Admin.Countries.Update;

/// <summary>Updates an existing country after validating ISO code uniqueness.</summary>
public static partial class UpdateCountry
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates the country exists and ISO code is unique, then applies updates.</summary>
        /// <param name="command">The command containing the update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated country details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=country!=null && IsoCode unique (excl. self), post=country updated, throws=DbUpdateException
            var request = command.Request;

            // Load: Country exists for the given Id.
            var country = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.Id == command.Id, cancellationToken: cancellationToken);

            if (country is null)
                return CountryResult.Failure.NotFound;

            // Validate: No duplicate ISO code with another country (case-insensitive).
            var duplicateCountry = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c =>
                    c.IsoCode == request.IsoCode &&
                    c.Id != command.Id, cancellationToken: cancellationToken);

            if (duplicateCountry is not null)
                return CountryResult.Failure.IsoCodeDuplicate;

            // Update: Apply request data to country entity.
            request.MapToDomain(country: country);

            dbContext.Set<Country>().Update(entity: country);
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return updated country as response.
            return country.MapToDetail<Response>();
        }
    }
}