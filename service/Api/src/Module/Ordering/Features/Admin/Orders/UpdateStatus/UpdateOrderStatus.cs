using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

    /// <summary>Handles UpdateOrderStatus feature.</summary>
    public static partial class UpdateOrderStatus
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Check: Find the existing entity.
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            var request = command.Request;

            // Update: Apply status transition.
            switch (request.Status)
            {
                case OrderStatus.Placed when entity.Status == OrderStatus.Draft:
                    // Update: Modify entity properties.
                    entity.Status = OrderStatus.Placed;
                    // Update: Modify entity properties.
                    entity.CompletedAtUtc = DateTimeOffset.UtcNow;
                    break;
                case OrderStatus.Canceled:
                    // Update: Modify entity properties.
                    entity.Status = OrderStatus.Canceled;
                    // Update: Modify entity properties.
                    entity.CanceledAtUtc = DateTimeOffset.UtcNow;
                    break;
                default:
                    return OrderResult.Errors.InvalidStatusTransition;
            }

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Success.
            if (entity.Status == OrderStatus.Placed)
                // Log: Record operation outcome.
                OrderLoggers.Placed(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);
            else if (entity.Status == OrderStatus.Canceled)
                // Log: Record operation outcome.
                OrderLoggers.Canceled(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
