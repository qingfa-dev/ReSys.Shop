using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Events;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;

namespace Module.Ordering.Features.Admin.Orders.Cancel;
/// <summary>Handles CancelOrderAdmin feature.</summary>
public static partial class CancelOrderAdmin
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the order exists.
            var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.Id);

            // Update: Cancel the order.
            var parsed = Guid.TryParse(currentUser.UserId, out var userId);
            var result = order.Cancel(userId);
            if (result.IsFailure)
                return result.Failures;

            // Raise: Order canceled domain event.
            order.AddDomainEvent(new OrderCanceledEvent(
                order.Id,
                order.Number,
                order.UserId!.Value,
                order.Email ?? string.Empty,
                order.CanceledAtUtc!.Value,
                currentUser.UserId));

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated entity as response.
            return order.MapToDetail<Response>();
        }
    }
}
