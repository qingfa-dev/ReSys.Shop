namespace Module.Inventory.Domain.StockTransfers;

public static class StockTransferValidation
{
    /// <summary>
    /// Validates that the source and destination locations differ.
    /// </summary>
    public static Result ValidateLocationsDiffer(Guid sourceLocationId, Guid destinationLocationId)
    {
        if (sourceLocationId == destinationLocationId)
            return StockTransferResult.Failure.SameLocation;

        return Result.Ok();
    }

    public static IRuleBuilderOptions<T, TransferState> ApplyStateRules<T>(this IRuleBuilder<T, TransferState> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(StockTransferResult.Failure.InvalidState.Code)
            .WithMessage(StockTransferResult.Failure.InvalidState.Message);
    }
}