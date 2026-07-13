using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Update;

/// <summary>Updates an existing shipping rate with PATCH semantics.</summary>
public static partial class UpdateShippingRate
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Applies partial updates to the shipping rate and persists changes.</summary>
        /// <param name="command">The command containing the shipping rate ID and update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated shipping rate details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=rate!=null, post=rate updated, throws=DbUpdateException
            // Load: Find the shipping rate by ID
            var rate = await dbContext.Set<ShippingRate>()
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (rate is null)
                return ShippingRateResult.Errors.NotFound(command.Id);

            // Update: Apply partial update mapping
            var result = command.Request.MapUpdateToDomain(rate);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated rate as response
            return rate.MapToDetail<Response>();
        }
    }
}