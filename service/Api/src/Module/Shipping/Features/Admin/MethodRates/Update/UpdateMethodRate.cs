using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.MethodRates.Update;
/// <summary>Updates an existing shipping rate.</summary>
public static partial class UpdateMethodRate
{
    public sealed record Command(Guid RateId, Request Request) : ICommand<Response>;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles updating a method rate.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated rate response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            _ = logger;
            // Check: Get rate by ID.
            var rate = await dbContext.Set<ShippingRate>().FirstOrDefaultAsync(r => r.Id == command.RateId, cancellationToken);
            if (rate is null) return (Result<Response>)ShippingRateResult.Errors.NotFound(command.RateId);
            // Validate: Business rules.
            // Update: Apply changes to the rate.
            if (command.Request.Name is not null) rate.Name = command.Request.Name;
            if (command.Request.Cost.HasValue) { rate.Cost = command.Request.Cost.Value; rate.FinalPrice = command.Request.Cost.Value; }
            if (command.Request.DeliveryRange is not null) rate.DeliveryRange = command.Request.DeliveryRange;
            if (command.Request.MinWeight.HasValue) rate.MinWeight = command.Request.MinWeight.Value;
            if (command.Request.MaxWeight.HasValue) rate.MaxWeight = command.Request.MaxWeight.Value;
            if (command.Request.FreeShippingThreshold.HasValue) rate.FreeShippingThreshold = command.Request.FreeShippingThreshold.Value;
            rate.ModifiedAtUtc = DateTimeOffset.UtcNow;
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Log: Operation success.
            // Map: Return response.
            return new Response { Id = rate.Id, Name = rate.Name, Cost = rate.Cost, FinalPrice = rate.FinalPrice, ModifiedAtUtc = rate.ModifiedAtUtc };
        }
    }
}
