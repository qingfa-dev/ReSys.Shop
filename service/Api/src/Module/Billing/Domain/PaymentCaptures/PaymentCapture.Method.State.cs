namespace Module.Billing.Domain.PaymentCaptures;

public static partial class PaymentCaptureMethod
{
    #region State Transitions
    // Update: Checkout → Processing — validates transition via CanTransitionTo
    public static Result Process(this Payment payment)
    {
        if (!CanTransitionTo(PaymentRecordState.Processing))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Processing);

        payment.State = PaymentRecordState.Processing;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Processed(payment.Number));

        bool CanTransitionTo(PaymentRecordState target) => target switch
        {
            PaymentRecordState.Processing => payment.State is PaymentRecordState.Checkout,
            _ => false
        };
    }

    // Update: Processing → Pending — requires state to be Processing
    public static Result Pend(this Payment payment)
    {
        if (payment.State is not PaymentRecordState.Processing)
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Pending);

        payment.State = PaymentRecordState.Pending;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Pended(payment.Number));
    }

    // Update: Processing/Pending → Completed — returns AlreadyCompleted error if already completed
    public static Result Complete(this Payment payment)
    {
        if (payment.State is PaymentRecordState.Completed)
            return PaymentCaptureResult.Failure.AlreadyCompleted;

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        payment.CapturedAmount = payment.Amount;
        payment.State = PaymentRecordState.Completed;
        payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Completed(payment.Number));
    }

    // Update: Checkout/Processing/Pending → Failed — idempotent if already failed
    public static Result Fail(this Payment payment)
    {
        if (payment.State is PaymentRecordState.Failed)
            return PaymentCaptureResult.Failure.AlreadyFailed;

        if (payment.State is not (PaymentRecordState.Checkout or PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Failed);

        payment.State = PaymentRecordState.Failed;
        payment.FailedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Failed(payment.Number));
    }

    // Update: Processing/Pending → Void — idempotent if already voided
    public static Result Void(this Payment payment)
    {
        if (payment.State is PaymentRecordState.Void)
            return PaymentCaptureResult.Failure.AlreadyVoided;

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Void);

        payment.State = PaymentRecordState.Void;
        payment.VoidedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Voided(payment.Number));
    }

    // Update: Any non-terminal state → Disputed — idempotent if already disputed
    public static Result Dispute(this Payment payment)
    {
        if (payment.State is PaymentRecordState.Disputed)
            return PaymentCaptureResult.Failure.AlreadyDisputed;

        if (payment.State is PaymentRecordState.Void or PaymentRecordState.Invalid)
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Disputed);

        payment.State = PaymentRecordState.Disputed;
        payment.DisputedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // Update: Failed/Void → Invalid — idempotent if already invalid
    public static Result Invalidate(this Payment payment)
    {
        if (payment.State is PaymentRecordState.Invalid)
            return Result.Ok();

        if (payment.State is not (PaymentRecordState.Failed or PaymentRecordState.Void))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Invalid);

        payment.State = PaymentRecordState.Invalid;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion

    #region Capture Logic
    // Check: Credit/refund only allowed when state is Completed or Disputed
    public static bool CreditAllowed(this Payment payment)
        => payment.State is PaymentRecordState.Completed or PaymentRecordState.Disputed;

    // Compute: Amount remaining to capture — 0 when fully captured
    public static decimal UncapturedAmount(this Payment payment)
        => payment.CapturedAmount >= payment.Amount ? 0 : payment.Amount - payment.CapturedAmount;

    // Check: Can capture — Processing/Pending, positive amount, within remaining authorized
    public static bool CanCapture(this Payment payment, decimal amount)
        => payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending
           && amount > 0
           && amount <= payment.Amount - payment.CapturedAmount;

    // Update: Capture amount — accumulates CapturedAmount; Completed only when fully captured
    public static Result Capture(this Payment payment, decimal amount)
    {
        if (!payment.CanCapture(amount))
            return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        payment.CapturedAmount += amount;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        if (payment.CapturedAmount >= payment.Amount)
        {
            payment.State = PaymentRecordState.Completed; // fully captured
            payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        }
        return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount));
    }

    // Check: Can refund — Completed/Disputed, positive, within captured - already refunded
    public static bool CanRefund(this Payment payment, decimal amount)
        => payment.State is PaymentRecordState.Completed or PaymentRecordState.Disputed
           && amount > 0
           && amount <= payment.CapturedAmount - payment.RefundedAmount;

    // Update: Refund amount — increments RefundedAmount (state stays Completed)
    public static Result Refund(this Payment payment, decimal amount)
    {
        if (!payment.CanRefund(amount))
        {
            return payment.State is not (PaymentRecordState.Completed or PaymentRecordState.Disputed)
                ? PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed)
                : PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        }
        payment.RefundedAmount += amount;
        payment.RefundedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Credited(payment.Number, amount));
    }

    // Update: Reconcile RefundedAmount with the gateway's authoritative total — monotonic, capped at CapturedAmount
    public static Result ReconcileRefunded(this Payment payment, decimal totalRefunded)
    {
        if (payment.State is not (PaymentRecordState.Completed or PaymentRecordState.Disputed))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        if (totalRefunded <= 0 || totalRefunded <= payment.RefundedAmount)
            return Result.Ok();

        if (totalRefunded > payment.CapturedAmount)
            return PaymentCaptureResult.Failure.AmountExceedsAuthorized;

        payment.RefundedAmount = totalRefunded;
        payment.RefundedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion

}