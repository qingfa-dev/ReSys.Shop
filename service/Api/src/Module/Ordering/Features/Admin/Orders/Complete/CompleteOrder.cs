using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Complete;
public static partial class CompleteOrder
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            if (order.Status != OrderStatus.Placed)
                return OrderResult.Errors.InvalidStatusTransition;

            order.CheckoutState = CheckoutState.Complete;
            order.CompletedAtUtc = DateTimeOffset.UtcNow;
            order.ModifiedAtUtc = DateTimeOffset.UtcNow;
            order.ModifiedBy = currentUser.UserName;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
