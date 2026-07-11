using Module.Payment.Domain.Gateways;

namespace Module.Payment.Domain.PaymentCaptures;

public static partial class PaymentCaptureMethod
{
    #region Process Entry Points
    public static Task<Result> ProcessAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (gateway.AutoCapture)
            return PurchaseAsync(payment, gateway, options, cancellationToken);
        return AuthorizeAsync(payment, gateway, options, cancellationToken);
    }
    #endregion

    #region Authorization
    public static async Task<Result> AuthorizeAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        var preconditions = payment.HandlePaymentPreconditions(gateway);
        if (preconditions.IsFailure)
            return preconditions;

        payment.StartedProcessing();

        return await GatewayActionAsync(payment, gateway, options,
            (amount, src, opts, ct) => gateway.AuthorizeAsync(amount, src, opts, ct),
            PaymentRecordState.Pending, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Purchase
    public static async Task<Result> PurchaseAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        var preconditions = payment.HandlePaymentPreconditions(gateway);
        if (preconditions.IsFailure)
            return preconditions;

        payment.StartedProcessing();

        var result = await GatewayActionAsync(payment, gateway, options,
            (amount, src, opts, ct) => gateway.PurchaseAsync(amount, src, opts, ct),
            PaymentRecordState.Completed, cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
            payment.CaptureEventCreated = true;

        return result;
    }
    #endregion

    #region Confirmation
    public static Task<Result> ConfirmAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken cancellationToken = default)
    {
        payment.StartedProcessing();

        if (gateway.AutoCapture && payment.State != PaymentRecordState.Completed)
        {
            payment.State = PaymentRecordState.Completed;
            return Task.FromResult(Result.Ok(PaymentCaptureResult.Success.Completed(payment.Number)));
        }

        if (payment.State == PaymentRecordState.Checkout || payment.State == PaymentRecordState.Processing)
        {
            payment.State = PaymentRecordState.Pending;
            return Task.FromResult(Result.Ok(PaymentCaptureResult.Success.Pended(payment.Number)));
        }

        return Task.FromResult(Result.Ok());
    }
    #endregion

    #region Capture
    public static async Task<Result> CaptureAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        if (payment.State == PaymentRecordState.Completed)
            return PaymentCaptureResult.Failure.AlreadyCompleted;

        amount ??= payment.Amount;

        if (!payment.CanCapture(amount.Value))
            return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);

        payment.StartedProcessing();

        var gatewayResult = await gateway.CaptureAsync(amount.Value, payment.ResponseCode, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult.ToBase();

        var response = gatewayResult.Value;
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.State = PaymentRecordState.Completed;
            payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
            return Result.Ok(PaymentCaptureResult.Success.Captured(payment.Number, amount.Value));
        }

        payment.State = PaymentRecordState.Failed;
        return Result.Failure(PaymentCaptureResult.Failure.CaptureFailed(response.Message));
    }
    #endregion

    #region Void
    public static async Task<Result> VoidTransactionAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, object? source = null, CancellationToken cancellationToken = default)
    {
        if (payment.State == PaymentRecordState.Void)
            return Result.Ok();

        if (string.IsNullOrEmpty(payment.ResponseCode))
        {
            payment.State = PaymentRecordState.Void;
            return Result.Ok(PaymentCaptureResult.Success.Voided(payment.Number));
        }

        var gatewayResult = gateway.PaymentProfilesSupported && source is not null
            ? await gateway.VoidAsync(payment.ResponseCode, source, options, cancellationToken).ConfigureAwait(false)
            : await gateway.VoidAsync(payment.ResponseCode, null, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult.ToBase();

        var response = gatewayResult.Value;
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
            payment.State = PaymentRecordState.Void;
            return Result.Ok(PaymentCaptureResult.Success.Voided(payment.Number));
        }

        return Result.Failure(PaymentCaptureResult.Failure.VoidFailed(response.Message));
    }
    #endregion

    #region Cancel
    public static async Task<Result> CancelAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken cancellationToken = default)
    {
        var gatewayResult = await gateway.VoidAsync(payment.ResponseCode, payment, new GatewayOptions
        {
            Email = string.Empty,
            Customer = string.Empty,
            OrderId = payment.OrderId.ToString(),
            PaymentId = payment.Number,
            IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number)
        }, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult.ToBase();

        var response = gatewayResult.Value;
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.State = PaymentRecordState.Void;
            return Result.Ok(PaymentCaptureResult.Success.Voided(payment.Number));
        }

        payment.State = PaymentRecordState.Failed;
        return PaymentCaptureResult.Failure.CancelFailed(response.Message);
    }
    #endregion

    #region Credit
    public static async Task<Result> CreditAsync(this PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CreditAllowed())
            return PaymentCaptureResult.Failure.CreditNotAllowed;

        var gatewayResult = await gateway.RefundAsync(amount, payment.ResponseCode, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult.Errors;

        var response = gatewayResult.Value;
        payment.RecordGatewayResponse(response);

        if (response.Success)
            return Result.Ok(PaymentCaptureResult.Success.Credited(payment.Number, amount));

        return PaymentCaptureResult.Failure.CreditFailed(response.Message);
    }
    #endregion

    #region Helper Methods
    private static Result HandlePaymentPreconditions(this PaymentCapture payment, IPaymentGatewayActionProvider gateway)
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

    private static void StartedProcessing(this PaymentCapture payment)
    {
        if (payment.State == PaymentRecordState.Checkout)
            payment.State = PaymentRecordState.Processing;
    }

    private static async Task<Result> GatewayActionAsync(
        PaymentCapture payment,
        IPaymentGatewayActionProvider gateway,
        GatewayOptions options,
        Func<decimal, object?, GatewayOptions, CancellationToken, Task<Result<PaymentGatewayResponse>>> action,
        PaymentRecordState successState,
        CancellationToken cancellationToken)
    {
        var source = payment.SourceType is not null ? new { Id = payment.SourceId, Type = payment.SourceType } : null;
        var gatewayResult = await action(payment.Amount, source, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult.Errors;

        var response = gatewayResult.Value;
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
            payment.State = successState;
            return Result.Ok(successState == PaymentRecordState.Pending
                ? PaymentCaptureResult.Success.Pended(payment.Number)
                : PaymentCaptureResult.Success.Completed(payment.Number));
        }

        payment.State = PaymentRecordState.Failed;
        return PaymentCaptureResult.Failure.GatewayError(response.Message);
    }

    private static void RecordGatewayResponse(this PaymentCapture payment, PaymentGatewayResponse response)
    {
        payment.AvsResponse = response.AvsResultCode;
        payment.CvvResponseCode = response.CvvResultCode;
        payment.CvvResponseMessage = response.CvvResultMessage;
    }
    #endregion
}
