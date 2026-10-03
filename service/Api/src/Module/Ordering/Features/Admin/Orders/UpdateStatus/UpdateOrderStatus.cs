using Module.Billing.Features.Shared.Commands;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Extensions;
using Module.Shipping.Domain.Shipments;

using Shared.Application.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.UpdateStatus;

/// <summary>Transitions an order to a new status (e.g., canceled) with side effects — payment voiding, shipment cancellation, inventory release and audit logging.</summary>
public static partial class UpdateOrderStatus
{
    public sealed record Command(Guid Id, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        ISender sender,
        IStockReservationService stockReservation)
        : ICommandHandler<Command>
    {
        /// <summary>Applies a status transition (currently only cancel) to an order with payment voiding, shipment cancellation, inventory release and logging.</summary>
        /// <param name="command">The command containing the order ID and target status.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            // Check: Find the order with full detail (line items, shipments, payment captures) for the cancel side effects.
            var entity = await dbContext.Set<Order>()
                .IncludeOrderDetail()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return OrderResult.Errors.NotFound(command.Id);

            var request = command.Request;

            // Update: Apply status transition based on target status.
            switch (request.Status)
            {
                case OrderStatus.Placed when entity.Status == OrderStatus.Draft:
                    var prereqCheck = entity.ValidateCheckoutPrerequisites();
                    if (prereqCheck.IsFailure)
                        return prereqCheck.Errors;
                    var finalizeResult = entity.Finalize();
                    if (finalizeResult.IsFailure)
                        return finalizeResult.Errors;
                    break;
                case OrderStatus.Canceled when entity.Status == OrderStatus.Placed:
                    var cancelResult = entity.Cancel(Guid.TryParse(currentUser.UserId, out var uid) ? uid : Guid.Empty);
                    if (cancelResult.IsFailure)
                        return cancelResult.Errors;

                    entity.RecomputePaymentState();

                    // In-process: Cancel all shipments BEFORE dispatching the gateway void — a failed
                    // shipment guard returns without touching gateway payments.
                    foreach (var shipment in entity.Shipments)
                    {
                        var shipmentCancelResult = shipment.Cancel();
                        if (shipmentCancelResult.IsFailure)
                            return shipmentCancelResult.Errors;
                    }

                    entity.ShipmentState = ShipmentState.Canceled;

                    // Call: Void pending payments — fire-and-forget on failure (order already canceled).
                    var voidResult = await sender.Send(
                        new VoidOrderPaymentsCommand
                        {
                            OrderId = entity.Id,
                            Reason = OrderConstant.CancelReasons.Admin
                        },
                        cancellationToken);
                    if (voidResult.IsFailure)
                        OrderLoggers.VoidPaymentsFailed(logger, entity.Id, string.Join("; ", voidResult.Errors.Select(f => f.Message)));

                    // Release: Return consumed stock for the placed order.
                    var returnResult = await stockReservation.ReturnConsumedForOrderAsync(entity.Id, cancellationToken);
                    if (returnResult.IsFailure)
                        return returnResult.Errors;
                    break;
                default:
                    return OrderResult.Errors.InvalidStatusTransition;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record status transition in audit log for compliance trail.
            if (entity.Status == OrderStatus.Canceled)
                OrderLoggers.Canceled(logger, Number: entity.Number, Id: entity.Id, ActionBy: currentUser.UserName);

            return Result.Ok(OrderResult.Success.Updated(command.Id));
        }
    }
}
