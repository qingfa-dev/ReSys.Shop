using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Complete;
/// <summary>Marks a placed order as complete, finalizing the order lifecycle with timestamp and user attribution.</summary>
public static partial class CompleteOrder
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        /// <summary>Transitions the order to complete state, setting the completion timestamp and modifier identity.</summary>
        /// <param name="command">The command containing the order ID to complete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The completed order response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            if (order.Status != OrderStatus.Placed)
                return OrderResult.Errors.InvalidStatusTransition;

            order.CheckoutState = CheckoutState.Complete;
            order.CompletedAtUtc = DateTimeOffset.UtcNow;
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;
            order.ModifiedBy = currentUser.UserName;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
