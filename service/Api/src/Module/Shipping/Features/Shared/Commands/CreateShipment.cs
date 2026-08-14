using Module.Shipping.Domain.Shipments;

namespace Module.Shipping.Features.Shared.Commands;

public sealed record CreateShipmentCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid ShippingMethodId { get; init; }
}

/// <summary>Creates a Pending shipment for a placed order.</summary>
public sealed class CreateShipmentCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateShipmentCommand>
{
    public async Task<Result> Handle(
        CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        var createResult = ShipmentMethod.Create(command.OrderId, command.ShippingMethodId);
        if (createResult.IsFailure)
            return createResult.Errors;

        dbContext.Set<Shipment>().Add(createResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
