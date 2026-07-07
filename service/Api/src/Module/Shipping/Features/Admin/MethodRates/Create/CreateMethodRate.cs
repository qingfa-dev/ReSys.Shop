using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.MethodRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.MethodRates.Create;
/// <summary>Creates a new shipping rate for a method.</summary>
public static partial class CreateMethodRate
{
    public sealed record Command(Guid MethodId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles creating a method rate.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created rate response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Build the shipping rate via mapping.
            var createResult = request.MapToDomain(command.MethodId);
            if (createResult.IsFailure)
                return createResult.Failures;

            var rate = createResult.Value;

            // Persist: Save changes.
            dbContext.Set<ShippingRate>().Add(rate);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Operation success.
            ShippingRateLoggers.Created(logger, rate.Name, rate.Id, rate.ShipmentId);

            // Map: Return response.
            return Result<Response>.Ok(rate.MapToDetail<Response>(), "Shipping rate created successfully.");
        }
    }
}
