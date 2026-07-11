using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

namespace Module.Location.Features.Admin.Countries.Create;

/// <summary>Creates a new country after validating ISO code uniqueness.</summary>
public static partial class CreateCountry
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Checks for duplicate ISO codes, maps the request, and persists the new country.</summary>
        /// <param name="command">The command containing the country request data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created country or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && IsoCode unique, post=country persisted, throws=DbUpdateException
            var request = command.Request;

            // Check: No duplicate country with same ISO code (case-insensitive)
            var existingEntity = await dbContext.Set<Country>()
                .FirstOrDefaultAsync(predicate: c => c.IsoCode == request.IsoCode,
                    cancellationToken: cancellationToken);

            // Validate: ISO code uniqueness business rule
            if (existingEntity is not null)
                return CountryResult.Failure.IsoCodeDuplicate;

            // Create: Map request to domain entity
            var entity = request.MapToDomain();

            // Add: Persist new country to database
            dbContext.Set<Country>().Add(entity: entity);
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return created country as response
            return Result<Response>.Created(entity.MapToListItem<Response>());
        }
    }
}