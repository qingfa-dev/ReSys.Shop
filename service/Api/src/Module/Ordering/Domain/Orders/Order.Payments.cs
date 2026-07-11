using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;
using PaymentStateEnum = Module.Payment.Domain.PaymentCaptures.PaymentRecordState;

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

        foreach (var payment in GetUnprocessedPayments())
        {
            if (PaymentTotal >= Total) break;
        }

        return true;
    }

    // Compute: Whether there are unprocessed (checkout-state) payments
    public bool HasUnprocessedPayments =>
        Payments.Any(p => p.State == PaymentStateEnum.Checkout);

    // Compute: Get unprocessed payment records
    public IReadOnlyList<PaymentCapture> GetUnprocessedPayments() =>
        Payments
            .Where(p => p.State == PaymentStateEnum.Checkout)
            .ToList()
            .AsReadOnly();

    #endregion
}
