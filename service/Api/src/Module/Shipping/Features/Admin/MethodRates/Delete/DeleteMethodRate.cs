using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.MethodRates.Delete;
/// <summary>Deletes a shipping rate by ID.</summary>
public static partial class DeleteMethodRate
{
    public sealed record Command(Guid RateId) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext, ILogger<CommandHandler> logger) : ICommandHandler<Command>
    {
        /// <summary>Handles deleting a method rate.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            _ = logger;
            // Check: Get rate by ID.
            var rate = await dbContext.Set<ShippingRate>().FirstOrDefaultAsync(r => r.Id == command.RateId, cancellationToken);
            if (rate is null) return ShippingRateResult.Errors.NotFound(command.RateId);
            // Validate: Business rules.
            // Remove: Delete the rate.
            dbContext.Set<ShippingRate>().Remove(rate);
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);
            // Log: Operation success.
            // Map: Return response.
            return Result.Ok();
        }
    }
}
