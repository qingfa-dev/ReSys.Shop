using Module.Payment.Domain.Gateways;

namespace Module.Payment.Domain.Payments;

public static class PaymentFactory
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
    public static Result<Payment> Create(
        decimal amount,
        Guid paymentMethodId,
        Guid orderId,
        Guid? sourceId = null,
        string? sourceType = null)
    {
        // Validate: Payment amount must be greater than zero
        if (amount <= 0)
        {
            return PaymentResult.Failure.AmountMustBePositive;
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Number = GeneratePaymentNumber(),
            Amount = amount,
            State = PaymentConstant.Defaults.State,
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
    public static Result Process(this Payment payment)
    {
        if (!CanTransitionTo(PaymentState.Processing))
        {
            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Processing);
        }

        payment.State = PaymentState.Processing;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentResult.Success.Processed(payment.Number));

        bool CanTransitionTo(PaymentState target) => target switch
        {
            PaymentState.Processing => payment.State is PaymentState.Checkout,
            _ => false
        };
    }

    /// <summary>
    /// Transitions the payment to the Pending state awaiting gateway confirmation.
    /// </summary>
    /// <param name="payment">The payment to pend. Must be in Processing state.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must be in Processing state to transition to Pending
    public static Result Pend(this Payment payment)
    {
        if (payment.State is not PaymentState.Processing)
        {
            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Pending);
        }

        payment.State = PaymentState.Pending;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentResult.Success.Pended(payment.Number));
    }

    /// <summary>
    /// Marks the payment as completed after successful gateway confirmation.
    /// </summary>
    /// <param name="payment">The payment to complete. Must be in Processing or Pending state and not already completed.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must not already be completed and must be in Processing or Pending state
    public static Result Complete(this Payment payment)
    {
        if (payment.State is PaymentState.Completed)
        {
            return PaymentResult.Failure.AlreadyCompleted;
        }

        if (payment.State is not (PaymentState.Processing or PaymentState.Pending))
        {
            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Completed);
        }

        payment.State = PaymentState.Completed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentResult.Success.Completed(payment.Number));
    }

    /// <summary>
    /// Marks the payment as failed due to a gateway error or declined transaction.
    /// </summary>
    /// <param name="payment">The payment to fail. Must be in Checkout, Processing, or Pending state and not already failed.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must not already be failed and must be in Checkout, Processing, or Pending state
    public static Result Fail(this Payment payment)
    {
        if (payment.State is PaymentState.Failed)
        {
            return PaymentResult.Failure.AlreadyFailed;
        }

        if (payment.State is not (PaymentState.Checkout or PaymentState.Processing or PaymentState.Pending))
        {
            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Failed);
        }

        payment.State = PaymentState.Failed;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentResult.Success.Failed(payment.Number));
    }

    /// <summary>
    /// Voids the payment to prevent capture before settlement.
    /// </summary>
    /// <param name="payment">The payment to void. Must be in Processing or Pending state and not already voided.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must not already be voided and must be in Processing or Pending state
    public static Result Void(this Payment payment)
    {
        if (payment.State is PaymentState.Void)
        {
            return PaymentResult.Failure.AlreadyVoided;
        }

        if (payment.State is not (PaymentState.Processing or PaymentState.Pending))
        {
            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Void);
        }

        payment.State = PaymentState.Void;
        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentResult.Success.Voided(payment.Number));
    }

    /// <summary>
    /// Invalidates a failed or voided payment as the terminal state for unrecoverable payments.
    /// </summary>
    /// <param name="payment">The payment to invalidate. Must be in Failed or Void state.</param>
    /// <returns>A result indicating success or an invalid state transition error.</returns>
    // Enforce: Payment must be in Failed or Void state to transition to Invalid
    public static Result Invalidate(this Payment payment)
    {
        if (payment.State is PaymentState.Invalid)
        {
            return Result.Ok();
        }

        if (payment.State is not (PaymentState.Failed or PaymentState.Void))
        {
            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Invalid);
        }

        payment.State = PaymentState.Invalid;
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
    public static bool CreditAllowed(this Payment payment)
    {
        return payment.State is PaymentState.Completed;
    }

    /// <summary>
    /// Calculates the amount that has not yet been captured against this payment.
    /// </summary>
    /// <param name="payment">The payment to calculate for.</param>
    /// <returns>The full payment amount if not yet completed, or zero once completed.</returns>
    // Compute: Uncaptured amount is the full payment amount before completion; zero after completion
    public static decimal UncapturedAmount(this Payment payment)
    {
        return payment.State is PaymentState.Completed ? 0 : payment.Amount;
    }

    /// <summary>
    /// Determines whether a specified amount can be captured against this payment.
    /// </summary>
    /// <param name="payment">The payment to check.</param>
    /// <param name="amount">The amount to capture. Must be positive and within the payment amount.</param>
    /// <returns>True if the payment is in a capturable state and the amount is valid.</returns>
    // Compute: Capture is allowed when payment is Processing or Pending and amount is positive and within the authorized amount
    public static bool CanCapture(this Payment payment, decimal amount)
    {
        return payment.State is PaymentState.Processing or PaymentState.Pending
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
    public static Result Capture(this Payment payment, decimal amount)
    {
        if (!payment.CanCapture(amount))
        {
            if (amount > payment.Amount)
            {
                return PaymentResult.Failure.AmountExceedsAuthorized;
            }

            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Completed);
        }

        return Result.Ok(PaymentResult.Success.Captured(payment.Number, amount));
    }

    /// <summary>
    /// Determines whether a refund can be issued against this completed payment.
    /// </summary>
    /// <param name="payment">The payment to check.</param>
    /// <param name="amount">The amount to refund. Must be positive and not exceed the payment amount.</param>
    /// <returns>True if the payment is completed and the refund amount is valid.</returns>
    // Compute: Refund is allowed only when payment is Completed and amount is positive and within authorized amount
    public static bool CanRefund(this Payment payment, decimal amount)
    {
        return payment.State is PaymentState.Completed
            && amount > 0
            && amount <= payment.Amount;
    }

    /// <summary>
    /// Refunds the specified amount against a completed payment.
    /// </summary>
    /// <param name="payment">The payment to refund from. Must be in Completed state.</param>
    /// <param name="amount">The amount to refund.</param>
    /// <returns>A result indicating success or an error if the payment state or amount is invalid.</returns>
    // Enforce: Payment must be completed and the refund amount must not exceed the authorized amount
    public static Result Refund(this Payment payment, decimal amount)
    {
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentState.Completed)
                return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Completed);

            return PaymentResult.Failure.AmountExceedsAuthorized;
        }

        payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok(PaymentResult.Success.Credited(payment.Number, amount));
    }
    #endregion Capture Logic

    #region Async Gateway Processing

    /// <summary>
    /// Processes the payment via the gateway -- authorizes or purchases depending on auto_capture.
    /// </summary>
    // Contract: pre=payment!=null && gateway!=null, post=payment.State is Pending or Completed or Failed
    public static Task<Result> ProcessAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.ProcessAsync(payment, gateway, options, cancellationToken);
    }

    /// <summary>
    /// Authorizes the payment amount against the payment source via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Checkout && gateway!=null, post=payment.State==Pending or Failed
    public static Task<Result> AuthorizeAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.AuthorizeAsync(payment, gateway, options, cancellationToken);
    }

    /// <summary>
    /// Purchases (authorize + capture) the payment amount via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Checkout && gateway!=null, post=payment.State==Completed or Failed
    public static Task<Result> PurchaseAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.PurchaseAsync(payment, gateway, options, cancellationToken);
    }

    /// <summary>
    /// Captures the specified amount via the gateway after validating state preconditions.
    /// </summary>
    // Enforce: Payment must be capturable and the amount must not exceed the authorized amount
    public static async Task<Result> CaptureAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CanCapture(amount))
        {
            if (amount > payment.Amount)
                return PaymentResult.Failure.AmountExceedsAuthorized;

            return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Completed);
        }

        return await PaymentProcessing.CaptureAsync(payment, gateway, options, amount, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Voids the payment transaction via the gateway.
    /// </summary>
    // Contract: pre=payment.State is Processing or Pending, post=payment.State==Void or Failed
    public static Task<Result> VoidAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (payment.State is PaymentState.Void)
            return Task.FromResult<Result>(PaymentResult.Failure.AlreadyVoided);

        if (payment.State is not (PaymentState.Processing or PaymentState.Pending))
            return Task.FromResult<Result>(PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Void));

        return PaymentProcessing.VoidTransactionAsync(payment, gateway, options, null, cancellationToken);
    }

    /// <summary>
    /// Cancels the payment via the gateway cancel action.
    /// </summary>
    // Contract: post=payment.State==Void or Failed
    public static Task<Result> CancelAsync(this Payment payment, IPaymentGatewayActionProvider gateway, CancellationToken cancellationToken = default)
    {
        return PaymentProcessing.CancelAsync(payment, gateway, cancellationToken);
    }

    /// <summary>
    /// Refunds the specified amount via the gateway after validating state preconditions.
    /// </summary>
    // Enforce: Payment must be completed and the refund amount must not exceed the authorized amount
    public static async Task<Result> RefundAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentState.Completed)
                return PaymentResult.Failure.InvalidStateTransition(payment.State, PaymentState.Completed);

            return PaymentResult.Failure.AmountExceedsAuthorized;
        }

        var result = await PaymentProcessing.CreditAsync(payment, gateway, options, amount, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        return result;
    }

    #endregion Async Gateway Processing
}