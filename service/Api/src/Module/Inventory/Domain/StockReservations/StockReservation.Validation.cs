namespace Module.Inventory.Domain.StockReservations;

public static class StockReservationValidation
{
    public static IRuleBuilderOptions<T, int> ApplyQuantityRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(StockReservationResult.Errors.QuantityZero.Code)
            .WithMessage(StockReservationResult.Errors.QuantityZero.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyReasonRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockReservationConstant.Constraints.MaxReasonLength)
            .WithErrorCode(StockReservationResult.Errors.ReasonTooLong.Code)
            .WithMessage(StockReservationResult.Errors.ReasonTooLong.Message);
    }

    public static IRuleBuilderOptions<T, ReservationState> ApplyStateTransitionRules<T>(
        this IRuleBuilder<T, ReservationState> ruleBuilder)
    {
        return ruleBuilder
            .Must(state => state is ReservationState.Reserved or ReservationState.Fulfilled or ReservationState.Released or ReservationState.Expired)
            .WithErrorCode(StockReservationResult.Errors.InvalidStateTransition.Code)
            .WithMessage(StockReservationResult.Errors.InvalidStateTransition.Message);
    }
}