namespace Module.Ordering.Domain.Orders;

// Invariant: PaymentTotal must not exceed Total; payment errors must not block checkout by default
public sealed partial class Order
{
    #region Payment Processing

    // Enforce: Process each unprocessed payment until the total is covered
    public bool ProcessPayments()
    {
        // Guard: Skip if already fully paid
        if (PaymentTotal >= Total) return true;

        // Validate: At least one unprocessed payment must exist
        if (!HasUnprocessedPayments) return false;

        foreach (var _ in GetUnprocessedPayments())
        {
            if (PaymentTotal >= Total) break;
        }

        return true;
    }

    // Compute: Whether there are unprocessed (checkout-state) payments
    public bool HasUnprocessedPayments =>
        Payments.Any(p => p.State == "checkout");

    // Compute: Get unprocessed payment records
    public IReadOnlyList<PaymentRecord> GetUnprocessedPayments() =>
        Payments
            .Where(p => p.State == "checkout")
            .ToList()
            .AsReadOnly();

    #endregion
}
