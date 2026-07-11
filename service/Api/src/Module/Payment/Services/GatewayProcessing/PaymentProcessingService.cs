using Module.Payment.Services.Gateways;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Services.GatewayProcessing;

public sealed class PaymentProcessingService : IPaymentProcessingService
{
    public Task<Result<PaymentProcessingResult>> ProcessAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        if (gateway.AutoCapture)
            return PurchaseAsync(payment, gateway, options, ct);
        return AuthorizeAsync(payment, gateway, options, ct);
    }

    public async Task<Result<PaymentProcessingResult>> CaptureAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken ct = default)
    {
        if (payment.State == PaymentRecordState.Completed)
            return ProcessingResult.Errors.AlreadyCompleted;

        amount ??= payment.Amount;

        if (!payment.CanCapture(amount.Value))
            return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        StartedProcessing(payment);

        var gatewayResult = await gateway.CaptureAsync(amount.Value, payment.ResponseCode, options, ct).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.State = PaymentRecordState.Completed;
        payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
        return ProcessingResult.Success.Captured(payment.Number, amount.Value);
    }

    public Task<Result<PaymentProcessingResult>> VoidAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        if (payment.State is PaymentRecordState.Void)
            return Task.FromResult(ProcessingResult.Success.Voided(payment.Number));

        if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            return Task.FromResult<Result<PaymentProcessingResult>>(ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Void));

        return VoidTransactionAsync(payment, gateway, options, null, ct);
    }

    public async Task<Result<PaymentProcessingResult>> RefundAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default)
    {
        if (!payment.CanRefund(amount))
        {
            if (payment.State is not PaymentRecordState.Completed)
                return ProcessingResult.Errors.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

            return ProcessingResult.Errors.AmountExceedsAuthorized;
        }

        var result = await CreditAsync(payment, gateway, options, amount, ct).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            payment.RefundedAmount += amount;
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        return result;
    }

    public async Task<Result<PaymentProcessingResult>> VoidTransactionAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, object? source = null, CancellationToken ct = default)
    {
        if (payment.State == PaymentRecordState.Void)
            return ProcessingResult.Success.Voided(payment.Number);

        if (string.IsNullOrEmpty(payment.ResponseCode))
        {
            payment.State = PaymentRecordState.Void;
            return ProcessingResult.Success.Voided(payment.Number);
        }

        var gatewayResult = gateway.PaymentProfilesSupported && source is not null
            ? await gateway.VoidAsync(payment.ResponseCode, source, options, ct).ConfigureAwait(false)
            : await gateway.VoidAsync(payment.ResponseCode, null, options, ct).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
        payment.State = PaymentRecordState.Void;
        return ProcessingResult.Success.Voided(payment.Number);
    }

    public Task<Result<PaymentProcessingResult>> ConfirmAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default)
    {
        StartedProcessing(payment);

        if (gateway.AutoCapture && payment.State != PaymentRecordState.Completed)
        {
            payment.State = PaymentRecordState.Completed;
            return Task.FromResult(ProcessingResult.Success.ConfirmCompleted(payment.Number));
        }

        if (payment.State == PaymentRecordState.Checkout || payment.State == PaymentRecordState.Processing)
        {
            payment.State = PaymentRecordState.Pending;
            return Task.FromResult(ProcessingResult.Success.ConfirmPended(payment.Number));
        }

        return Task.FromResult(Result<PaymentProcessingResult>.Ok(new PaymentProcessingResult()));
    }

    #region Private Methods
    private static void StartedProcessing(PaymentCapture payment)
    {
        if (payment.State == PaymentRecordState.Checkout)
            payment.State = PaymentRecordState.Processing;
    }

    private static void RecordGatewayResponse(PaymentCapture payment, PaymentGatewayResponse response)
    {
        payment.AvsResponse = response.AvsResultCode;
        payment.CvvResponseCode = response.CvvResultCode;
        payment.CvvResponseMessage = response.CvvResultMessage;
    }

    private async Task<Result<PaymentProcessingResult>> AuthorizeAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
        var precondition = HandlePaymentPreconditions(payment, gateway);
        if (precondition.IsFailure)
            return Result<PaymentProcessingResult>.Failure(precondition.Errors[0]);

        StartedProcessing(payment);

        return await GatewayActionAsync(payment, gateway, options,
            (amount, src, opts, t) => gateway.AuthorizeAsync(amount, src, opts, t),
            PaymentRecordState.Pending, ct).ConfigureAwait(false);
    }

    private async Task<Result<PaymentProcessingResult>> PurchaseAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
    {
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

        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        payment.State = PaymentRecordState.Void;
        return ProcessingResult.Success.Voided(payment.Number);
    }

    private async Task<Result<PaymentProcessingResult>> CreditAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken ct = default)
    {
        if (!payment.CreditAllowed())
            return ProcessingResult.Errors.CreditNotAllowed;

        var gatewayResult = await gateway.RefundAsync(amount, payment.ResponseCode, options, ct).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return Result<PaymentProcessingResult>.Failure(gatewayResult.Errors[0]);

        var response = gatewayResult.Value;
        RecordGatewayResponse(payment, response);
        return ProcessingResult.Success.Credited(payment.Number, amount);
    }

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
