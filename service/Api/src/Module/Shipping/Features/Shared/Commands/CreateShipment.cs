using Module.Shipping.Domain.Shipments;
using Module.Shipping.Services;

namespace Module.Shipping.Features.Shared.Commands;

public sealed record CreateShipmentCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid ShippingMethodId { get; init; }
}

/// <summary>Creates a Pending shipment for a placed order.</summary>
public sealed class CreateShipmentCommandHandler(
    IApplicationDbContext dbContext,
    ShipmentFulfillmentSyncService syncService)
    : ICommandHandler<CreateShipmentCommand>
{
    public async Task<Result> Handle(
        CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        // Guard: Idempotent — a shipment for this order + shipping method already exists.
        var exists = await dbContext.Set<Shipment>()
            .AnyAsync(s => s.OrderId == command.OrderId && 
                           s.ShippingMethodId == command.ShippingMethodId, cancellationToken);
        if (exists)
            return Result.Ok();

        var createResult = ShipmentMethod.Create(command.OrderId, command.ShippingMethodId);
        if (createResult.IsFailure)
            return createResult.Errors;

        dbContext.Set<Shipment>().Add(createResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        await syncService.SyncOrderFulfillmentAsync(command.OrderId, cancellationToken);
        return Result.Ok();
    }
}
