using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.AdvanceCheckoutState;

public sealed class AdvanceCheckoutStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<AdvanceCheckoutStateCommand>
{
    public async Task<Result> Handle(
        AdvanceCheckoutStateCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .FirstOrDefaultAsync(
                x => x.Id == command.CartId && x.Status == OrderStatus.Draft,
                cancellationToken);

        if (cart is null)
            return OrderResult.Errors.NotFound(command.CartId);

        var result = cart.AdvanceCheckoutState(command.TargetState);
        if (result.IsFailure)
            return result.Errors;

        // Note: entering PickPaymentMethod means "method picked", not "processing" —
        // PaymentProcessingAt is stamped via RecordOrderPaymentState{Processing} instead.
        if (command.PaymentMethodId.HasValue)
        {
            var pmResult = cart.SelectPaymentMethod(command.PaymentMethodId.Value);
            if (pmResult.IsFailure)
                return pmResult.Errors;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(OrderResult.Success.CheckoutAdvanced(cart.Id));
    }
}
