using Module.Payment.Domain.Gateways;

namespace Module.Payment.Domain.PaymentCaptures;

public static partial class PaymentCaptureMethod
{
    #region State Transitions
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

    public static Result Pend(this PaymentCapture payment)
    {
        if (payment.State is not PaymentRecordState.Processing)
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Pending);

        payment.State = PaymentRecordState.Pending;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Pended(payment.Number));
    }

    public static Result Complete(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Completed)
            return PaymentCaptureResult.Failure.AlreadyCompleted;

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        payment.State = PaymentRecordState.Completed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Completed(payment.Number));
    }

    public static Result Fail(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Failed)
            return PaymentCaptureResult.Failure.AlreadyFailed;

        if (payment.State is not (PaymentRecordState.Checkout or PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Failed);

        payment.State = PaymentRecordState.Failed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Failed(payment.Number));
    }

    public static Result Void(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Void)
            return PaymentCaptureResult.Failure.AlreadyVoided;

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Void);

        payment.State = PaymentRecordState.Void;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Voided(payment.Number));
    }

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
    public static bool CreditAllowed(this PaymentCapture payment)
        => payment.State is PaymentRecordState.Completed;

    public static decimal UncapturedAmount(this PaymentCapture payment)
        => payment.State is PaymentRecordState.Completed ? 0 : payment.Amount;

    public static bool CanCapture(this PaymentCapture payment, decimal amount)
        => payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending
           && amount > 0 && amount <= payment.Amount;

    public static Result Capture(this PaymentCapture payment, decimal amount)
    {
        if (!payment.CanCapture(amount))
        {
            return amount > payment.Amount
                ? PaymentCaptureResult.Failure.AmountExceedsAuthorized
                : PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
        }
        return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount));
    }

    public static bool CanRefund(this PaymentCapture payment, decimal amount)
        => payment.State is PaymentRecordState.Completed
           && amount > 0 && (payment.Amount - payment.RefundedAmount) >= amount;

    public static Result Refund(this PaymentCapture payment, decimal amount)
    {
        if (!payment.CanRefund(amount))
        {
            return payment.State is not PaymentRecordState.Completed
                ? PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed)
                : PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        }

        payment.RefundedAmount += amount;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Credited(payment.Number, amount));
    }
    #endregion

    #region Gateway Convenience Wrappers
    public static Task<Result> VoidAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (payment.State is PaymentRecordState.Void)
            return Task.FromResult<Result>(PaymentCaptureResult.Failure.AlreadyVoided);

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return Task.FromResult<Result>(PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Void));

        return VoidTransactionAsync(payment, gateway, options, null, cancellationToken);
    }

    public static async Task<Result> RefundAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentRecordState.Completed)
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        }

        var result = await CreditAsync(payment, gateway, options, amount, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            payment.RefundedAmount += amount;
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        return result;
    }
    #endregion
}
