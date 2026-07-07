using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Features.Admin.Orders.AddAdjustment;

/// <summary>Handles adding an adjustment to an order.</summary>
    /// <summary>Handles AddOrderAdjustment feature.</summary>
    public static partial class AddOrderAdjustment
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
            var entity = await dbContext.Set<Order>()
                .Include(x => x.Adjustments)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            var request = command.Request;

            // Create: Build a new adjustment.
            var adjustment = new Adjustment
            {
                Id = Guid.NewGuid(),
                OrderId = entity.Id,
                Label = request.Label,
                Amount = request.Amount,
                Included = request.Inclusive,
                AdjustableId = entity.Id,
                AdjustableType = "Order",
                SourceId = entity.Id,
                SourceType = "Manual",
                Eligible = true,
                State = "open",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                CreatedBy = currentUser.UserName
            };

            // Create: Persist new entity.
            dbContext.Set<Adjustment>().Add(adjustment);
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);
            if (logger.IsEnabled(LogLevel.Debug))
                // Log: Record operation outcome.
                logger.LogDebug("[Adjustment.Created]: {Label} ({Amount}) for Order {OrderId} by {ActionBy}",
                    request.Label, request.Amount, entity.Id, currentUser.UserName);

            return Result.Ok();
        }
    }
}
