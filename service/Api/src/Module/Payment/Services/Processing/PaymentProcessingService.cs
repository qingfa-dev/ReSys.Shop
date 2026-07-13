using Module.Payment.Services.Provider;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Services.Processing;

public sealed class PaymentProcessingService : IPaymentProcessingService
{
    // Contract: pre=payment!=null && gateway!=null, post=Result<PaymentProcessingResult>
    public Task<Result<PaymentProcessingResult>> ProcessAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        // Skip: Auto-capture gateway routes to Purchase — otherwise Authorize
        if (gateway.AutoCapture)
            return PurchaseAsync(payment, gateway, options, ct);
        return AuthorizeAsync(payment, gateway, options, ct);
    }

    // Contract: pre=payment!=null, post=payment.State==Completed || Result.IsFailure
    public async Task<Result<PaymentProcessingResult>> CaptureAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken ct = default)
    {
        // Check: Already completed — idempotency guard
        if (payment.State == PaymentRecordState.Completed)
            return ProcessingResult.Errors.AlreadyCompleted;

        amount ??= payment.Amount;

        // Check: Payment does not allow capture at current state or amount
        if (!payment.CanCapture(amount.Value))
            return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        StartedProcessing(payment);

        // Call: Gateway capture API — Stripe PaymentIntent capture
        var gatewayResult = await gateway.CaptureAsync(amount.Value, payment.ResponseCode, options, ct).ConfigureAwait(false);

        // Catch: Gateway failure — propagate error without mutating state
        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
        return ProcessingResult.Success.Captured(payment.Number, amount.Value);
    }

    // Contract: pre=payment!=null, post=payment.State==Void || Result.IsFailure
    public Task<Result<PaymentProcessingResult>> VoidAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: Already voided — idempotency guard
        if (payment.State is PaymentRecordState.Void)
            return Task.FromResult(ProcessingResult.Success.Voided(payment.Number));

        // Check: Only Processing or Pending states can be voided
        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return Task.FromResult<Result<PaymentProcessingResult>>(ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Void));

        return VoidTransactionAsync(payment, gateway, options, null, ct);
    }

    // Contract: pre=payment!=null && amount>0, post=RefundedAmount incremented || Result.IsFailure
    public async Task<Result<PaymentProcessingResult>> RefundAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default)
    {
        // Check: Payment state and refund amount must be valid
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentRecordState.Completed)
                return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            return ProcessingResult.Errors.AmountExceedsAuthorized;
        }

        // Call: Gateway refund API — Stripe Refund Create
        var result = await CreditAsync(payment, gateway, options, amount, ct).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            payment.RefundedAmount += amount;
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        return result;
    }

    // Contract: pre=payment!=null, post=payment.State==Void || Result.IsFailure
    public async Task<Result<PaymentProcessingResult>> VoidTransactionAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, object? source = null, CancellationToken ct = default)
    {
        // Check: Already voided — idempotency guard
        if (payment.State == PaymentRecordState.Void)
            return ProcessingResult.Success.Voided(payment.Number);

        // Skip: No gateway response code — void locally without gateway call
        if (string.IsNullOrEmpty(payment.ResponseCode))
        {
            payment.State = PaymentRecordState.Void;
            return ProcessingResult.Success.Voided(payment.Number);
        }

        // Call: Gateway void API — stripe.payment_intent.cancel
        var gatewayResult = gateway.PaymentProfilesSupported && source is not null
            ? await gateway.VoidAsync(payment.ResponseCode, source, options, ct).ConfigureAwait(false)
            : await gateway.VoidAsync(payment.ResponseCode, null, options, ct).ConfigureAwait(false);

        // Catch: Gateway failure — propagate error without mutating state
        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
        payment.State = PaymentRecordState.Void;
        return ProcessingResult.Success.Voided(payment.Number);
    }

    // Contract: pre=payment!=null, post=payment.State==Completed|Pending || Result.IsFailure
    public Task<Result<PaymentProcessingResult>> ConfirmAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default)
    {
        StartedProcessing(payment);

        // Check: Auto-capture gateway — transition directly to Completed
        if (gateway.AutoCapture && payment.State != PaymentRecordState.Completed)
        {
            payment.State = PaymentRecordState.Completed;
            return Task.FromResult(ProcessingResult.Success.ConfirmCompleted(payment.Number));
        }

        // Check: Checkout or Processing state — transition to Pending
        if (payment.State == PaymentRecordState.Checkout || payment.State == PaymentRecordState.Processing)
        {
            payment.State = PaymentRecordState.Pending;
            return Task.FromResult(ProcessingResult.Success.ConfirmPended(payment.Number));
        }

        return Task.FromResult(Result<PaymentProcessingResult>.Ok(new PaymentProcessingResult()));
    }

    #region Private Methods
    // Update: Set state to Processing if currently Checkout
    private static void StartedProcessing(PaymentCapture payment)
    {
        if (payment.State == PaymentRecordState.Checkout)
            payment.State = PaymentRecordState.Processing;
    }

    // Map: Gateway response fields onto payment entity
    private static void RecordGatewayResponse(PaymentCapture payment, PaymentGatewayResponse response)
    {
        payment.AvsResponse = response.AvsResultCode;
        payment.CvvResponseCode = response.CvvResultCode;
        payment.CvvResponseMessage = response.CvvResultMessage;
        payment.IntentClientSecret = response.ClientSecret;
    }

    // Call: Gateway authorize — transition to Pending
    private async Task<Result<PaymentProcessingResult>> AuthorizeAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: Source and state preconditions
        var precondition = HandlePaymentPreconditions(payment, gateway);
        if (precondition.IsFailure)
            return Result<PaymentProcessingResult>.Failure(precondition.Errors[0]);

        StartedProcessing(payment);

        return await GatewayActionAsync(payment, gateway, options,
            (amount, src, opts, t) => gateway.AuthorizeAsync(amount, src, opts, t),
            PaymentRecordState.Pending, ct).ConfigureAwait(false);
    }

    // Call: Gateway purchase (authorize+capture) — transition to Completed
    private async Task<Result<PaymentProcessingResult>> PurchaseAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        // Check: Source and state preconditions
        var precondition = HandlePaymentPreconditions(payment, gateway);
        if (precondition.IsFailure)
            return Result<PaymentProcessingResult>.Failure(precondition.Errors[0]);

        StartedProcessing(payment);

        var result = await GatewayActionAsync(payment, gateway, options,
            (amount, src, opts, t) => gateway.PurchaseAsync(amount, src, opts, t),
            PaymentRecordState.Completed, ct).ConfigureAwait(false);

        if (result.IsSuccess)
            payment.CaptureEventCreated = true;

        return result;
    }

    // Call: Gateway cancel/void — no gateway response code means local void only
    private async Task<Result<PaymentProcessingResult>> CancelAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default)
    {
        var gatewayResult = await gateway.VoidAsync(payment.ResponseCode, payment, new GatewayOptions
        {
            Email = string.Empty,
            Customer = string.Empty,
            OrderId = payment.OrderId.ToString(),
            PaymentId = payment.Number,
            IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number)
        }, ct).ConfigureAwait(false);

        // Catch: Gateway failure — propagate error
        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.State = PaymentRecordState.Void;
        return ProcessingResult.Success.Voided(payment.Number);
    }

    // Call: Gateway refund — only allowed when payment is Completed
    private async Task<Result<PaymentProcessingResult>> CreditAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default)
    {
        // Check: Payment must be in Completed state for credit
        if (!payment.CreditAllowed())
            return ProcessingResult.Errors.CreditNotAllowed;

        var gatewayResult = await gateway.RefundAsync(amount, payment.ResponseCode, options, ct).ConfigureAwait(false);

        // Catch: Gateway failure — propagate error
        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        return ProcessingResult.Success.Credited(payment.Number, amount);
    }

    // Check: Source-required gateways must have source and not be already processing
    private static Result HandlePaymentPreconditions(PaymentCapture payment, IPaymentGatewayActionProvider gateway)
    {
        if (gateway.SourceRequired)
        {
            if (payment.SourceId is null || string.IsNullOrEmpty(payment.SourceType))
                return PaymentCaptureResult.Failure.ProcessingSourceRequired;

            if (payment.State == PaymentRecordState.Processing)
                return PaymentCaptureResult.Failure.ProcessingAlreadyProcessing;
        }
        return Result.Ok();
    }

    // Call: Generic gateway action with state transition — shared by Authorize, Purchase
    private static async Task<Result<PaymentProcessingResult>> GatewayActionAsync(
        PaymentCapture payment,
        IPaymentGatewayActionProvider gateway,
        GatewayOptions options,
        Func<decimal, object?, GatewayOptions, CancellationToken, Task<Result<PaymentGatewayResponse>>> action,
        PaymentRecordState successState,
        CancellationToken ct)
    {
        var source = payment.SourceType is not null ? new { Id = payment.SourceId, Type = payment.SourceType } : null;
        var gatewayResult = await action(payment.Amount, source, options, ct).ConfigureAwait(false);

        // Catch: Gateway failure — propagate error
        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
        payment.State = successState;
        return successState == PaymentRecordState.Pending
            ? ProcessingResult.Success.Pended(payment.Number)
            : ProcessingResult.Success.Completed(payment.Number);
    }
    #endregion
}