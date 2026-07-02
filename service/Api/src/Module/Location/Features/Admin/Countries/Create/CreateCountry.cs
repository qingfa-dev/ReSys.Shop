using Module.Location.Domain.Countries;
using Module.Location.Features.Admin.Countries.Shared.Mappings;

namespace Module.Location.Features.Admin.Countries.Create;

/// <summary>Handles creation of a new country.</summary>
public static partial class CreateCountry
{
    // Command
    /// <summary>Command to create a new country.</summary>
    public sealed record Command(Request Request) : ICommand<Response>;

    // Command Handler
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the create country command.</summary>
        /// <param name="command">The command containing the country request data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the created country.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
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

            // Persist: Add new country to database
            dbContext.Set<Country>().Add(entity: entity);
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return created country as response
            return entity.MapToListItem<Response>();
        }
    }
}