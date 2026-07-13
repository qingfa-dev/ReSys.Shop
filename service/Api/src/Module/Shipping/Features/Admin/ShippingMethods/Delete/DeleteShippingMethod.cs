using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingMethods.Delete;

/// <summary>Soft-deletes a shipping method, preventing new usage while preserving referential integrity.</summary>
public static partial class DeleteShippingMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Validates no associated shipping rates exist, then marks the method as deleted.</summary>
        /// <param name="command">The command identifying the shipping method to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or an error if not found or has associated rates.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=method!=null, post=method.IsDeleted==true, throws=DbUpdateException
            // Load: Find the shipping method by ID
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            // Check: Ensure no shipping rates are associated with this method
            var hasRates = await dbContext.Set<ShippingRate>()
                .AnyAsync(r => r.ShippingMethodId == command.Id, cancellationToken);

            if (hasRates)
                return ShippingMethodResult.Failure.HasAssociatedRates;

            // Soft Delete: Mark with audit trail
            method.IsDeleted = true;
            method.DeletedAtUtc = DateTimeOffset.UtcNow;
            method.DeletedBy = "System";

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}