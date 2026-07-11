using Module.Payment.Domain.Gateways;

namespace Module.Payment.Domain.PaymentCaptures;

public static class PaymentCaptureMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new payment for an order with the specified amount and payment method.
    /// </summary>
    /// <param name="amount">The payment amount. Must be greater than zero.</param>
    /// <param name="paymentMethodId">The identifier of the payment method.</param>
    /// <param name="orderId">The identifier of the associated order.</param>
    /// <param name="sourceId">Optional identifier of the payment source.</param>
    /// <param name="sourceType">Optional type name of the payment source.</param>
    /// <returns>A result containing the created payment or an amount validation error.</returns>
    // Contract: pre=amount>0 && paymentMethodId!=default && orderId!=default, post=payment.Id!=default && payment.State==Checkout
    public static Result<PaymentCapture> Create(
        decimal amount,
        Guid paymentMethodId,
        Guid orderId,
        Guid? sourceId = null,
        string? sourceType = null)
    {
        // Validate: Payment amount must be greater than zero
        if (amount <= 0)
        {
            return PaymentCaptureResult.Failure.AmountMustBePositive;
        }

        var payment = new PaymentCapture
        {
            Id = Guid.NewGuid(),
            Number = GeneratePaymentNumber(),
            Amount = amount,
            State = PaymentRecordState.Checkout,
            PaymentMethodId = paymentMethodId,
            OrderId = orderId,
            SourceId = sourceId,
            SourceType = sourceType,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return payment;
    }

    private static string GeneratePaymentNumber()
    {
        return $"PAY-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
    #endregion Factory Methods

    #region State Transitions
    /// <summary>
    /// Transitions the payment to the Processing state for gateway submission.
    /// </summary>
    /// <param name="payment">The payment to process. Must be in Checkout state.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must be in Checkout state to transition to Processing
    public static Result Process(this PaymentCapture payment)
    {
        if (!CanTransitionTo(PaymentRecordState.Processing))
        {
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Processing);
        }

        payment.State = PaymentRecordState.Processing;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentCaptureResult.Success.Processed(payment.Number));

        bool CanTransitionTo(PaymentRecordState target) => target switch
        {
            PaymentRecordState.Processing => payment.State is PaymentRecordState.Checkout,
            _ => false
        };
    }

    /// <summary>
    /// Transitions the payment to the Pending state awaiting gateway confirmation.
    /// </summary>
    /// <param name="payment">The payment to pend. Must be in Processing state.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must be in Processing state to transition to Pending
    public static Result Pend(this PaymentCapture payment)
    {
        if (payment.State is not PaymentRecordState.Processing)
        {
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Pending);
        }

        payment.State = PaymentRecordState.Pending;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentCaptureResult.Success.Pended(payment.Number));
    }

    /// <summary>
    /// Marks the payment as completed after successful gateway confirmation.
    /// </summary>
    /// <param name="payment">The payment to complete. Must be in Processing or Pending state and not already completed.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must not already be completed and must be in Processing or Pending state
    public static Result Complete(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Completed)
        {
            return PaymentCaptureResult.Failure.AlreadyCompleted;
        }

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
        {
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
        }

        payment.State = PaymentRecordState.Completed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentCaptureResult.Success.Completed(payment.Number));
    }

    /// <summary>
    /// Marks the payment as failed due to a gateway error or declined transaction.
    /// </summary>
    /// <param name="payment">The payment to fail. Must be in Checkout, Processing, or Pending state and not already failed.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must not already be failed and must be in Checkout, Processing, or Pending state
    public static Result Fail(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Failed)
        {
            return PaymentCaptureResult.Failure.AlreadyFailed;
        }

        if (payment.State is not (PaymentRecordState.Checkout or PaymentRecordState.Processing or PaymentRecordState.Pending))
        {
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Failed);
        }

        payment.State = PaymentRecordState.Failed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentCaptureResult.Success.Failed(payment.Number));
    }

    /// <summary>
    /// Voids the payment to prevent capture before settlement.
    /// </summary>
    /// <param name="payment">The payment to void. Must be in Processing or Pending state and not already voided.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must not already be voided and must be in Processing or Pending state
    public static Result Void(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Void)
        {
            return PaymentCaptureResult.Failure.AlreadyVoided;
        }

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
        {
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Void);
        }

        payment.State = PaymentRecordState.Void;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentCaptureResult.Success.Voided(payment.Number));
    }

    /// <summary>
    /// Invalidates a failed or voided payment as the terminal state for unrecoverable payments.
    /// </summary>
    /// <param name="payment">The payment to invalidate. Must be in Failed or Void state.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must be in Failed or Void state to transition to Invalid
    public static Result Invalidate(this PaymentCapture payment)
    {
        if (payment.State is PaymentRecordState.Invalid)
        {
            return Result.Ok();
        }

        if (payment.State is not (PaymentRecordState.Failed or PaymentRecordState.Void))
        {
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Invalid);
        }

        payment.State = PaymentRecordState.Invalid;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }
    #endregion State Transitions

    #region Capture Logic
    /// <summary>
    /// Determines whether a credit can be issued against this completed payment.
    /// </summary>
    /// <param name="payment">The payment to check.</param>
    /// <returns>True if the payment is in Completed state and eligible for credit.</returns>
    // Compute: Credit is allowed only when the payment has reached the Completed state
    public static bool CreditAllowed(this PaymentCapture payment)
    {
        return payment.State is PaymentRecordState.Completed;
    }

    /// <summary>
    /// Calculates the amount that has not yet been captured against this payment.
    /// </summary>
    /// <param name="payment">The payment to calculate for.</param>
    /// <returns>The full payment amount if not yet completed, or zero once completed.</returns>
    // Compute: Uncaptured amount is the full payment amount before completion; zero after completion
    public static decimal UncapturedAmount(this PaymentCapture payment)
    {
        return payment.State is PaymentRecordState.Completed ? 0 : payment.Amount;
    }

    /// <summary>
    /// Determines whether a specified amount can be captured against this payment.
    /// </summary>
    /// <param name="payment">The payment to check.</param>
    /// <param name="amount">The amount to capture. Must be positive and within the payment amount.</param>
    /// <returns>True if the payment is in a capturable state and the amount is valid.</returns>
    // Compute: Capture is allowed when payment is Processing or Pending and amount is positive and within the authorized amount
    public static bool CanCapture(this PaymentCapture payment, decimal amount)
    {
        return payment.State is PaymentRecordState.Processing or PaymentRecordState.Pending
            && amount > 0
            && amount <= payment.Amount;
    }

    /// <summary>
    /// Captures the specified amount against the payment, finalizing the transaction.
    /// </summary>
    /// <param name="payment">The payment to capture from.</param>
    /// <param name="amount">The amount to capture. Must be capturable per business rules.</param>
    /// <returns>A result indicating success or an error if the payment state or amount is invalid.</returns>
    // Enforce: Payment must be capturable and the amount must not exceed the authorized amount
    public static Result Capture(this PaymentCapture payment, decimal amount)
    {
        if (!payment.CanCapture(amount))
        {
            if (amount > payment.Amount)
            {
                return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
            }

            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
        }

        return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount));
    }

    /// <summary>
    /// Determines whether a refund can be issued against this completed payment.
    /// </summary>
    /// <param name="payment">The payment to check.</param>
    /// <param name="amount">The amount to refund. Must be positive and not exceed the payment amount.</param>
    /// <returns>True if the payment is completed and the refund amount is valid.</returns>
    // Compute: Refund is allowed only when Payment is Completed and amount is within remaining refundable
    public static bool CanRefund(this PaymentCapture payment, decimal amount)
    {
        return payment.State is PaymentRecordState.Completed
            && amount > 0
            && (payment.Amount - payment.RefundedAmount) >= amount;
    }

    /// <summary>
    /// Refunds the specified amount against a completed payment.
    /// </summary>
    /// <param name="payment">The payment to refund from. Must be in Completed state.</param>
    /// <param name="amount">The amount to refund.</param>
    /// <returns>A result indicating success or an error if the payment state or amount is invalid.</returns>
    // Enforce: Payment must be completed and the refund amount must not exceed the authorized amount
    public static Result Refund(this PaymentCapture payment, decimal amount)
    {
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentRecordState.Completed)
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        }

        payment.RefundedAmount += amount;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentCaptureResult.Success.Credited(payment.Number, amount));
    }
    #endregion Capture Logic

    #region Async Gateway Processing

    /// <summary>
    /// Processes the payment via the gateway -- authorizes or purchases depending on auto_capture.
    /// </summary>
    // Contract: pre=payment!=null && gateway!=null, post=payment.State is Pending or Completed or Failed
    public static Task<Result> ProcessAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.ProcessAsync(payment, gateway, options, cancellationToken);
    }

    /// <summary>
    /// Authorizes the payment amount against the payment source via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Checkout && gateway!=null, post=payment.State==Pending or Failed
    public static Task<Result> AuthorizeAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.AuthorizeAsync(payment, gateway, options, cancellationToken);
    }

    /// <summary>
    /// Purchases (authorize + capture) the payment amount via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Checkout && gateway!=null, post=payment.State==Completed or Failed
    public static Task<Result> PurchaseAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.PurchaseAsync(payment, gateway, options, cancellationToken);
    }

    /// <summary>
    /// Captures the specified amount via the gateway after validating state preconditions.
    /// </summary>
    // Enforce: Payment must be capturable and the amount must not exceed the authorized amount
    public static async Task<Result> CaptureAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CanCapture(amount))
        {
            if (amount > payment.Amount)
                return PaymentCaptureResult.Failure.AmountExceedsAuthorized;

            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
        }

        return await PaymentProcessing.CaptureAsync(payment, gateway, options, amount, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Voids the payment transaction via the gateway.
    /// </summary>
    // Contract: pre=payment.State is Processing or Pending, post=payment.State==Void or Failed
    public static Task<Result> VoidAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (payment.State is PaymentRecordState.Void)
            return Task.FromResult<Result>(PaymentCaptureResult.Failure.AlreadyVoided);

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return Task.FromResult<Result>(PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Void));

        return PaymentProcessing.VoidTransactionAsync(payment, gateway, options, null, cancellationToken);
    }

    /// <summary>
    /// Cancels the payment via the gateway cancel action.
    /// </summary>
    // Contract: post=payment.State==Void or Failed
    public static Task<Result> CancelAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.CancelAsync(payment, gateway, cancellationToken);
    }

    /// <summary>
    /// Refunds the specified amount via the gateway after validating state preconditions.
    /// </summary>
    // Enforce: Payment must be completed and the refund amount must not exceed the authorized amount
    public static async Task<Result> RefundAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentRecordState.Completed)
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            return PaymentCaptureResult.Failure.AmountExceedsAuthorized;
        }

        var result = await PaymentProcessing.CreditAsync(payment, gateway, options, amount, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            payment.RefundedAmount += amount;
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        return result;
    }

    #endregion Async Gateway Processing
}