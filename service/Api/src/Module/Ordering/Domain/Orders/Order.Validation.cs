using Shared.Application.Domain.Currencies;

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
                    CheckoutState.PickDeliveryMethod => o.BillAddressId != null && o.ShipAddressId != null,
                    CheckoutState.PickPaymentMethod => o.ShippingMethodId != null,
                    CheckoutState.Confirm => true,
                    CheckoutState.Complete => true,
                    _ => true
                };
            })
            .WithErrorCode(OrderResult.Errors.CannotAdvanceState.Code)
            .WithMessage(OrderResult.Errors.CannotAdvanceState.Message);
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
            .WithErrorCode(OrderResult.Errors.EmptyOrderCannotFinalize.Code)
            .WithMessage(OrderResult.Errors.EmptyOrderCannotFinalize.Message);
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
            .WithErrorCode(OrderResult.Errors.AlreadyCanceled.Code)
            .WithMessage(OrderResult.Errors.AlreadyCanceled.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyEmailRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .EmailAddress()
            .WithErrorCode(OrderResult.Errors.EmailInvalid.Code)
            .WithMessage(OrderResult.Errors.EmailInvalid.Message)
            .MaximumLength(OrderConstant.Constraints.MaxEmailLength)
            .WithErrorCode(OrderResult.Errors.EmailTooLong.Code)
            .WithMessage(OrderResult.Errors.EmailTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string> ApplyCurrencyRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return SystemCurrencyValidation.ApplyCurrencyRules(ruleBuilder);
    }

    public static IRuleBuilderOptions<T, Guid?> ApplyBillAddressIdRules<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.BillAddressIdRequired.Code)
            .WithMessage(OrderResult.Errors.BillAddressIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyBillAddressIdRules<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.BillAddressIdRequired.Code)
            .WithMessage(OrderResult.Errors.BillAddressIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid?> ApplyShipAddressIdRules<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.ShipAddressIdRequired.Code)
            .WithMessage(OrderResult.Errors.ShipAddressIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyShipAddressIdRules<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.ShipAddressIdRequired.Code)
            .WithMessage(OrderResult.Errors.ShipAddressIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid?> ApplyShippingMethodIdRules<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.ShippingMethodIdRequired.Code)
            .WithMessage(OrderResult.Errors.ShippingMethodIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, Guid> ApplyShippingMethodIdRules<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.ShippingMethodIdRequired.Code)
            .WithMessage(OrderResult.Errors.ShippingMethodIdRequired.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySpecialInstructionsRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(OrderConstant.Constraints.MaxSpecialInstructionsLength)
            .WithErrorCode(OrderResult.Errors.NotesTooLong.Code)
            .WithMessage(OrderResult.Errors.NotesTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplySessionIdRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(OrderResult.Errors.SessionIdRequired.Code)
            .WithMessage(OrderResult.Errors.SessionIdRequired.Message)
            .MaximumLength(OrderConstant.Constraints.MaxSessionIdLength)
            .WithErrorCode(OrderResult.Errors.SessionIdTooLong.Code)
            .WithMessage(OrderResult.Errors.SessionIdTooLong.Message);
    }
}