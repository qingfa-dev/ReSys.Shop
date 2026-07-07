namespace Module.Ordering.Domain.Orders;

// Invariant: TotalAppliedStoreCredit >= 0; TotalAppliedStoreCredit <= Total
public sealed partial class Order
{
    #region Store Credit Queries

    // Compute: Whether the order is fully covered by store credit
    public bool CoveredByStoreCredit() =>
        UserId.HasValue && TotalAppliedStoreCredit > 0m && TotalAppliedStoreCredit >= Total;

    // Compute: Total amount of store credits applied to the order
    public decimal TotalAppliedStoreCredit =>
        Payments?
            .Where(p => p.IsStoreCredit && !p.HasInvalidState)
            .Sum(p => p.Amount) ?? 0m;

    // Compute: Whether the order is using store credit
    public bool UsingStoreCredit() => TotalAppliedStoreCredit > 0m;

    // Compute: Order total minus applicable store credit
    public decimal OrderTotalAfterStoreCredit() =>
        Total - TotalApplicableStoreCredit;

    // Compute: Total minus applied store credit
    public decimal TotalMinusStoreCredits() =>
        Total - TotalAppliedStoreCredit;

    // Compute: Total applicable store credit (capped at total)
    public decimal TotalApplicableStoreCredit =>
        CheckoutState is CheckoutState.Payment or CheckoutState.Confirm or CheckoutState.Complete
            ? TotalAppliedStoreCredit
            : Math.Min(Total, TotalAvailableStoreCredit);

    // Compute: Total available store credit — placeholder resolved via domain service
    #pragma warning disable CA1822
    public decimal TotalAvailableStoreCredit => 0m;

    // Compute: Whether store credit could be used for this order
    public bool CouldUseStoreCredit() =>
        TotalAvailableStoreCredit > 0m;

    #endregion
}
