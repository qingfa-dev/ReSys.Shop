using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shared.Mappings;
using Module.Shipping.Services;

namespace Module.Shipping.Features.Admin.Shipments.UpdateStatus;

/// <summary>Updates a shipment's status, applying the domain transition and syncing the order's fulfillment state.</summary>
public static partial class UpdateShipmentStatus
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext, 
        ShipmentFulfillmentSyncService syncService)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Applies the target status transition, persists, and mirrors the derived fulfillment state to Ordering.</summary>
        /// <param name="command">The command containing the shipment ID and the target status.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>The updated shipment details or a domain error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Find the shipment by ID
            var shipment = await dbContext.Set<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (shipment is null)
                return ShipmentResult.Errors.NotFound(command.Id);

            // Transition: Apply the domain transition only when the target status differs from the current one
            // (a tracking-only edit arrives with the current status and must not re-run the transition guard).
            bool statusChanged = command.Request.Status != shipment.Status;
            bool transitionedToShipped = statusChanged && command.Request.Status == ShipmentStatus.Shipped;

            if (statusChanged)
            {
                Result transitionResult = command.Request.Status switch
                {
                    ShipmentStatus.Ready => shipment.MarkReady(),
                    ShipmentStatus.Shipped => shipment.MarkShipped(command.Request.TrackingNumber ?? ""),
                    ShipmentStatus.Delivered => shipment.MarkDelivered(),
                    ShipmentStatus.Backorder => shipment.Backorder(),
                    ShipmentStatus.Canceled => shipment.Cancel(),
                    _ => Result.Failure(ShipmentResult.Errors.InvalidTransition(shipment.Status, command.Request.Status))
                };

                if (transitionResult.IsFailure)
                    return transitionResult.Errors;
            }

            // Update: Persist an independently-provided tracking number (tracking-only edit or a non-Shipped
            // transition). The Shipped transition already applied it via MarkShipped, so it is skipped there.
            if (!transitionedToShipped
                && command.Request.TrackingNumber is { Length: > 0 } tracking
                && !string.IsNullOrWhiteSpace(tracking))
            {
                var trackingResult = shipment.UpdateTrackingNumber(tracking);
                if (trackingResult.IsFailure)
                    return trackingResult.Errors;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Sync: Recompute the order's derived fulfillment state and mirror it to Ordering (best-effort)
            await syncService.SyncOrderFulfillmentAsync(shipment.OrderId, cancellationToken);

            return shipment.MapToDetail<Response>();
        }
    }
}
