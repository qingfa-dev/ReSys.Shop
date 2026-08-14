using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Shared;
using Module.Shipping.Services;

namespace Module.Shipping.Features.Admin.Shipments.UpdateStatus;

/// <summary>Updates a shipment's status, applying the domain transition and syncing the order's fulfillment state.</summary>
public static partial class UpdateShipmentStatus
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ShipmentFulfillmentSyncService syncService)
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
                return ShipmentResult.Errors.NotFound;

            // Transition: Apply the domain transition for the target status
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

            await dbContext.SaveChangesAsync(cancellationToken);

            // Sync: Recompute the order's derived fulfillment state and mirror it to Ordering (best-effort)
            await syncService.SyncOrderFulfillmentAsync(shipment.OrderId, cancellationToken);

            return new Response
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                Status = shipment.Status,
                TrackingNumber = shipment.TrackingNumber,
                ShippedAtUtc = shipment.ShippedAtUtc,
                DeliveredAtUtc = shipment.DeliveredAtUtc
            };
        }
    }

    // EXCEPTION: shipment status update request — no shared request model exists for shipments yet
    public sealed record Request
    {
        public ShipmentStatus Status { get; init; }
        public string? TrackingNumber { get; init; }
    }

    // EXCEPTION: shipment detail DTO — no shared shipment response model exists yet
    public sealed record Response
    {
        public Guid Id { get; init; }
        public Guid OrderId { get; init; }
        public ShipmentStatus Status { get; init; }
        public string? TrackingNumber { get; init; }
        public DateTimeOffset? ShippedAtUtc { get; init; }
        public DateTimeOffset? DeliveredAtUtc { get; init; }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Status)
                .IsInEnum()
                .WithMessage("A valid shipment status is required.");

            RuleFor(x => x.Request.TrackingNumber)
                .MaximumLength(200)
                .WithMessage("Tracking number must not exceed 200 characters.")
                .When(x => x.Request.TrackingNumber is not null);
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ShippingFeature.Admin.Shipments.UpdateStatus.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id, request), ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateShipmentStatus))
            .WithTags(ShippingFeature.Tags.Shipment)
            .HasPermission(ShippingFeature.Admin.Shipments.UpdateStatus.Permission)
            .WithSummary(ShippingFeature.Admin.Shipments.UpdateStatus.Summary)
            .WithDescription(ShippingFeature.Admin.Shipments.UpdateStatus.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
