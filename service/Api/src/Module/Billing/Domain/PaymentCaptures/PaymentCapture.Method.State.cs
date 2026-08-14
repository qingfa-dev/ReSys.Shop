namespace Module.Billing.Domain.PaymentCaptures;

public static partial class PaymentCaptureMethod
{
    #region State Transitions
    // Update: Checkout → Processing — validates transition via CanTransitionTo
    public static Result Process(this PaymentCapture payment)
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
    public static Result Pend(this PaymentCapture payment)
    {
        if (payment.State is not PaymentRecordState.Processing)
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Pending);

        payment.State = PaymentRecordState.Pending;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Pended(payment.Number));
    }

    // Update: Processing/Pending → Completed — returns AlreadyCompleted error if already completed
    public static Result Complete(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Completed)
            return PaymentCaptureResult.Failure.AlreadyCompleted;

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        payment.State = PaymentRecordState.Completed;
        payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Completed(payment.Number));
    }

    // Update: Checkout/Processing/Pending → Failed — idempotent if already failed
    public static Result Fail(this PaymentCapture payment)
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
    public static Result Void(this PaymentCapture payment)
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
    public static Result Dispute(this PaymentCapture payment)
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
    public static Result Invalidate(this PaymentCapture payment)
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
    // Check: Credit/refund only allowed when state is Completed
    public static bool CreditAllowed(this PaymentCapture payment)
        => payment.State is PaymentRecordState.Completed;

    // Compute: Amount remaining to capture — 0 if already completed
    public static decimal UncapturedAmount(this PaymentCapture payment)
        => payment.State is PaymentRecordState.Completed ? 0 : payment.Amount;

    // Check: Can capture — state must be Processing/Pending and amount positive and <= total
    public static bool CanCapture(this PaymentCapture payment, decimal amount)
        => payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending
           && amount > 0 && amount <= payment.Amount;

    // Update: Capture amount — validates CanCapture precondition, transitions to Completed
    public static Result Capture(this PaymentCapture payment, decimal amount)
    {
        if (!payment.CanCapture(amount))
        {
            return amount > payment.Amount
                ? PaymentCaptureResult.Failure.AmountExceedsAuthorized
                : PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
        }
        payment.State = PaymentRecordState.Completed;
        payment.CompletedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount));
    }

    // Check: Can refund — state must be Completed, amount positive and <= remaining
    public static bool CanRefund(this PaymentCapture payment, decimal amount)
        => payment.State is PaymentRecordState.Completed
           && amount > 0 && (payment.Amount - payment.RefundedAmount) >= amount;

    // Update: Refund amount — validates CanRefund precondition, increments RefundedAmount
    public static Result Refund(this PaymentCapture payment, decimal amount)
    {
        if (!payment.CanRefund(amount))
        {
            return payment.State is not PaymentRecordState.Completed
                ? PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed)
                : PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        }

        payment.RefundedAmount += amount;
        payment.RefundedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Credited(payment.Number, amount));
    }

    // Update: Reconcile RefundedAmount with the gateway's authoritative total.
    // Monotonic — the local total never decreases, so an admin refund racing a
    // charge.refunded webhook cannot double-count the same money. Accepts Disputed
    // payments too: a dispute-loss auto-refund from Stripe still reports a total.
    public static Result ReconcileRefunded(this PaymentCapture payment, decimal totalRefunded)
    {
        if (payment.State is not (PaymentRecordState.Completed or PaymentRecordState.Disputed))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        if (totalRefunded <= 0 || totalRefunded <= payment.RefundedAmount)
            return Result.Ok();

        payment.RefundedAmount = totalRefunded;
        payment.RefundedAtUtc = DateTimeOffset.UtcNow;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
    #endregion

}