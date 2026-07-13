using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingRates.Delete;

/// <summary>Hard-deletes a shipping rate.</summary>
public static partial class DeleteShippingRate
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Finds the rate by ID and removes it from the database.</summary>
        /// <param name="command">The command identifying the shipping rate to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or a not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=rate!=null, post=rate removed from DB, throws=DbUpdateException
            // Load: Find the shipping rate by ID
            var rate = await dbContext.Set<ShippingRate>()
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (rate is null)
                return ShippingRateResult.Errors.NotFound(command.Id);

            // Remove: Delete the rate from the database
            dbContext.Set<ShippingRate>().Remove(rate);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}