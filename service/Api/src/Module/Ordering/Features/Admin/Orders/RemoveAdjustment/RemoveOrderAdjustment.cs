using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.RemoveAdjustment;

    /// <summary>Handles RemoveOrderAdjustment feature.</summary>
    public static partial class RemoveOrderAdjustment
{
    public sealed record Command(Guid OrderId, Guid AdjustmentId) : ICommand;
    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            var adjustment = await dbContext.Set<Adjustment>()
                .FirstOrDefaultAsync(a => a.Id == command.AdjustmentId && a.OrderId == command.OrderId, cancellationToken);
            if (adjustment is null) return AdjustmentResult.Errors.NotFound(command.AdjustmentId);
            // Query: Retrieve data from database.
            var order = await dbContext.Set<Order>().Include(o => o.Adjustments).Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order is null) return OrderResult.Errors.NotFound(command.OrderId);
            // Remove: Detach entity from collection.
            order.Adjustments.Remove(adjustment);
            order.RecalculateTotals();
            // Remove: Delete entity from database.
            dbContext.Set<Adjustment>().Remove(adjustment);
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
