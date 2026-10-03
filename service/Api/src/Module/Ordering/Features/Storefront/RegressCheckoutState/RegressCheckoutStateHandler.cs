using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RegressCheckoutState;

public sealed class RegressCheckoutStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RegressCheckoutStateCommand>
{
    public async Task<Result> Handle(
        RegressCheckoutStateCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .FirstOrDefaultAsync(
                x => x.Id == command.CartId && x.Status == OrderStatus.Draft,
                cancellationToken);

        if (cart is null)
            return OrderResult.Errors.NotFound(command.CartId);

        var result = cart.RegressCheckoutState(command.TargetState);
        if (result.IsFailure)
            return result.Errors;

        // A regression (e.g. an expired checkout session) invalidates the picked
        // payment method — the customer must re-pick and re-create the intent.
        cart.ClearPaymentMethod();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(OrderResult.Success.CheckoutAdvanced(cart.Id));
    }
}
