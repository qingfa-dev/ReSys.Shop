using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Create;

/// <summary>Creates a new shipping rate for a shipping method.</summary>
public static partial class CreateShippingRate
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Maps the request to a domain entity and persists the new shipping rate.</summary>
        /// <param name="command">The command containing the shipping rate data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created shipping rate details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && command.Request!=null, post=shippingRate persisted,
            //           throws=DbUpdateException
            var request = command.Request;

            // Create: Map the request to a domain entity
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Errors;

            var rate = createResult.Value;

            dbContext.Set<ShippingRate>().Add(rate);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created rate as response
            return rate.MapToDetail<Response>();
        }
    }
}