namespace Module.Ordering.Domain.Orders;

// Validate: FluentValidation extension methods enforcing Order invariants
public static class OrderValidation
{
    public static IRuleBuilderOptions<T, CheckoutState> ApplyCheckoutStateTransitionRules<T>(
        this IRuleBuilder<T, CheckoutState> ruleBuilder)
    {
        return ruleBuilder
            .Must((order, state, context) =>
            {
                if (context.InstanceToValidate is not Order o)
                    return true;

                return state switch
                {
                    CheckoutState.Delivery => o.BillAddressId != null && o.ShipAddressId != null,
                    CheckoutState.Payment => o.ShippingMethodId != null,
                    CheckoutState.Confirm => true,
                    CheckoutState.Complete => true,
                    _ => true
                };
            })
            .WithErrorCode(OrderResult.Failure.CannotAdvanceState.Code)
            .WithMessage(OrderResult.Failure.CannotAdvanceState.Message);
    }

    public static IRuleBuilderOptions<T, OrderStatus> ApplyFinalizeRules<T>(
        this IRuleBuilder<T, OrderStatus> ruleBuilder)
    {
        return ruleBuilder
            .Must((order, status, context) =>
            {
                if (context.InstanceToValidate is not Order o)
                    return true;

                if (o.Status == OrderStatus.Canceled)
                    return false;

                if (o.LineItems.Count == 0)
                    return false;

                return o.Total >= 0;
            })
            .WithErrorCode(OrderResult.Failure.EmptyOrderCannotFinalize.Code)
            .WithMessage(OrderResult.Failure.EmptyOrderCannotFinalize.Message);
    }

    public static IRuleBuilderOptions<T, OrderStatus> ApplyCancelRules<T>(
        this IRuleBuilder<T, OrderStatus> ruleBuilder)
    {
        return ruleBuilder
            .Must((order, status, context) =>
            {
                if (context.InstanceToValidate is not Order o)
                    return true;

                return o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Draft;
            })
            .WithErrorCode(OrderResult.Failure.AlreadyCanceled.Code)
            .WithMessage(OrderResult.Failure.AlreadyCanceled.Message);
    }
}
