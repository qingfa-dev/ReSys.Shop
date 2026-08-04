using Module.Ordering.Domain.Orders;
using Shared.Application.Contracts.Ordering;

namespace Module.Ordering.Features.Storefront.Contracts.AdvanceCheckoutState;

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

        if (!Enum.TryParse<CheckoutState>(command.TargetState, ignoreCase: true, out var targetState))
            return OrderResult.Errors.CannotAdvanceState;

        var result = cart.AdvanceCheckoutState(targetState);
        if (result.IsFailure)
            return result.Errors;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(OrderResult.Success.CheckoutAdvanced(cart.Id));
    }
}
