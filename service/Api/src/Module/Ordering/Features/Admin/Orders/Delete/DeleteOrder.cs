using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Delete;
/// <summary>Soft-deletes a draft order by marking it as deleted, preventing further modification while retaining the record.</summary>
public static partial class DeleteOrder
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Soft-deletes a draft order by setting the deleted flag, preventing placed orders from being deleted.</summary>
        /// <param name="command">The command containing the order ID to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the order to delete.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            // Enforce: Cannot delete a placed order — only draft orders can be removed.
            if (order.Status is OrderStatus.Placed)
                return OrderResult.Errors.InvalidStatusForDelete;

            // Update: Soft-delete — mark as deleted with timestamp instead of hard removal.
            order.IsDeleted = true;
            order.DeletedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(OrderResult.Success.Deleted(command.Id));
        }
    }
}
