using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecordOrderPaymentState;

/// <summary>Stamps the order's payment timestamp for the reported payment state.</summary>
public sealed class RecordOrderPaymentStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RecordOrderPaymentStateCommand>
{
    public async Task<Result> Handle(
        RecordOrderPaymentStateCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Set<Order>()
            .Include(o => o.PaymentCaptures)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
        if (order is null)
            return OrderResult.Errors.NotFound(command.OrderId);

        var result = command.PaymentState switch
        {
            PaymentTimelineState.Completed => order.MarkPaymentCompleted(command.AtUtc),
            PaymentTimelineState.Failed => order.MarkPaymentFailed(command.AtUtc),
            PaymentTimelineState.Processing => order.MarkPaymentProcessing(command.AtUtc),
            _ => Result.Ok()
        };
        if (result.IsFailure)
            return result.Errors;

        if (command.PaymentState is PaymentTimelineState.Completed or PaymentTimelineState.Failed)
            order.RecomputePaymentState();

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
