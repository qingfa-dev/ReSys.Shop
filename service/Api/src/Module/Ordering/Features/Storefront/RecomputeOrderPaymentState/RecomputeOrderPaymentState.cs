using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecomputeOrderPaymentState;

/// <summary>Recomputes the order's derived payment state from its captures.</summary>
public sealed record RecomputeOrderPaymentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
}

public sealed class RecomputeOrderPaymentStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RecomputeOrderPaymentStateCommand>
{
    public async Task<Result> Handle(
        RecomputeOrderPaymentStateCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Set<Order>()
            .Include(o => o.PaymentCaptures)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
        if (order is null)
            return OrderResult.Errors.NotFound(command.OrderId);

        order.RecomputePaymentState();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
