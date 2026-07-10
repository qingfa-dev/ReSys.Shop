using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Delete;
/// <summary>Handles DeleteOrder feature.</summary>
public static partial class DeleteOrder
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the order exists.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            if (order.Status is OrderStatus.Placed)
                return Error.Validation("Order.Delete.InvalidStatus", "Only Draft or Expired orders can be deleted.");

            order.IsDeleted = true;
            order.DeletedAtUtc = DateTimeOffset.UtcNow;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
