using Microsoft.EntityFrameworkCore;
using Module.Shipping.Domain.Shipments;
using ShipmentDomain = Module.Shipping.Domain.Shipments.Shipment;

namespace Module.Ordering.Features.Admin.Orders.Update.Shipment;

/// <summary>Updates a shipment for an order (tracking, shipping method).</summary>
public static partial class UpdateOrderShipment
{
    public class Request
    {
        public string? Tracking { get; init; }
        public Guid? ShippingMethodId { get; init; }
    }

    public class Response
    {
        public Guid Id { get; init; }
        public string? Tracking { get; init; }
        public Guid? ShippingMethodId { get; init; }
        public DateTimeOffset? ModifiedAtUtc { get; init; }
    }

    public sealed record Command(Guid OrderId, Guid ShipmentId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var shipment = await dbContext.Set<ShipmentDomain>()
                .FirstOrDefaultAsync(
                    s => s.Id == command.ShipmentId && s.OrderId == command.OrderId,
                    cancellationToken);

            if (shipment is null)
                return (Result<Response>)ShipmentResult.Errors.NotFound(command.ShipmentId);

            if (command.Request.Tracking is not null)
                shipment.Tracking = command.Request.Tracking;

            if (command.Request.ShippingMethodId.HasValue)
                shipment.ShippingMethodId = command.Request.ShippingMethodId.Value;

            shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = shipment.Id,
                Tracking = shipment.Tracking,
                ShippingMethodId = shipment.ShippingMethodId,
                ModifiedAtUtc = shipment.ModifiedAtUtc
            };
        }
    }
}
