using Module.Payment.Domain.Gateways;

namespace Module.Payment.Domain.Payments;

/// <summary>
/// Payment processing operations -- port of Spree::Payment::Processing concern.
/// Delegates to a gateway action provider for actual gateway communication.
/// </summary>
public static class PaymentProcessing
{
    #region Process Entry Points

    /// <summary>
    /// Processes the payment by either authorizing or purchasing depending on auto_capture setting.
    /// </summary>
    // Contract: pre=payment!=null && gateway!=null, post=payment.State is Pending or Completed or Failed
    // Enforce: Auto-capture determines authorize vs purchase path
    public static Task<Result> ProcessAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        if (gateway.AutoCapture)
            return PaymentProcessing.PurchaseAsync(payment, gateway, options, cancellationToken);
        return PaymentProcessing.AuthorizeAsync(payment, gateway, options, cancellationToken);
    }

    #endregion Process Entry Points

    #region Authorization

    /// <summary>
    /// Authorizes the payment amount against the payment source via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Checkout && gateway!=null, post=payment.State==Pending or Failed
    // Call: Gateway authorize action
    public static async Task<Result> AuthorizeAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        var preconditions = payment.HandlePaymentPreconditions(gateway);
        if (preconditions.IsFailure)
            return preconditions;

        payment.StartedProcessing();

        return await PaymentProcessing.GatewayActionAsync(payment, gateway, options,
            (amount, src, opts, ct) => gateway.AuthorizeAsync(amount, src, opts, ct),
            PaymentState.Pending, cancellationToken).ConfigureAwait(false);
    }

    #endregion Authorization

    #region Purchase

    /// <summary>
    /// Purchases (authorize + capture) the payment amount via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Checkout && gateway!=null, post=payment.State==Completed or Failed
    // Call: Gateway purchase action; creates capture event on success
    public static async Task<Result> PurchaseAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken cancellationToken = default)
    {
        var preconditions = payment.HandlePaymentPreconditions(gateway);
        if (preconditions.IsFailure)
            return preconditions;

        payment.StartedProcessing();

        var result = await PaymentProcessing.GatewayActionInnerAsync(
            payment, gateway, options,
            (amount, src, opts, ct) => gateway.PurchaseAsync(amount, src, opts, ct),
            PaymentState.Completed,
            cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            payment.CaptureEventCreated = true;
        }

        return result;
    }

    #endregion Purchase

    #region Confirmation

    /// <summary>
    /// Confirms the payment -- completes if auto_capture and eligible, otherwise pends.
    /// </summary>
    // Contract: pre=payment.State==Checkout, post=payment.State is Completed or Pending
    // Enforce: Auto-capture determines complete vs pend path
    public static Task<Result> ConfirmAsync(this Payment payment, IPaymentGatewayActionProvider gateway, CancellationToken cancellationToken = default)
    {
        payment.StartedProcessing();

        if (gateway.AutoCapture && payment.State != PaymentState.Completed)
        {
            payment.State = PaymentState.Completed;
            return Task.FromResult(Result.Ok(PaymentResult.Success.Completed(payment.Number)));
        }

        if (payment.State == PaymentState.Checkout || payment.State == PaymentState.Processing)
        {
            payment.State = PaymentState.Pending;
            return Task.FromResult(Result.Ok(PaymentResult.Success.Pended(payment.Number)));
        }

        return Task.FromResult(Result.Ok());
    }

    #endregion Confirmation

    #region Capture

    /// <summary>
    /// Captures a previously authorized payment amount via the gateway.
    /// Supports partial captures -- creates a capture event for each capture.
    /// </summary>
    // Contract: pre=payment.State is Processing or Pending, post=payment.State==Completed or Failed
    // Call: Gateway capture action
    public static async Task<Result> CaptureAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        if (payment.State == PaymentState.Completed)
            return Result.Ok();

        amount ??= payment.Amount;

        payment.StartedProcessing();

        // Call: Capture the payment amount via gateway
        var gatewayResult = await gateway.CaptureAsync(amount.Value, payment.ResponseCode, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult;

        var response = gatewayResult.Value;

        // Log: Record capture response
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.State = PaymentState.Completed;
            payment.ResponseCode = response.Authorization ?? payment.ResponseCode;

            return Result.Ok(PaymentResult.Success.Captured(payment.Number, amount.Value));
        }

        payment.State = PaymentState.Failed;
        return Result.Failure(PaymentResult.Errors.CaptureFailed(response.Message));
    }

    #endregion Capture

    #region Void

    /// <summary>
    /// Voids a transaction via the gateway. Supports profile-based and standard void.
    /// </summary>
    // Contract: pre=payment.State is Processing or Pending, post=payment.State==Void or Failed
    // Call: Gateway void action with source (profile-based) or without
    public static async Task<Result> VoidTransactionAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, object? source = null, CancellationToken cancellationToken = default)
    {
        if (payment.State == PaymentState.Void)
            return Result.Ok();

        if (string.IsNullOrEmpty(payment.ResponseCode))
        {
            payment.State = PaymentState.Void;
            return Result.Ok(PaymentResult.Success.Voided(payment.Number));
        }

        // Call: Gateway void -- with source if profiles supported, otherwise standard
        var gatewayResult = gateway.PaymentProfilesSupported && source is not null
            ? await gateway.VoidAsync(payment.ResponseCode, source, options, cancellationToken).ConfigureAwait(false)
            : await gateway.VoidAsync(payment.ResponseCode, null, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult;

        var response = gatewayResult.Value;

        // Log: Record void response
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
            payment.State = PaymentState.Void;
            return Result.Ok(PaymentResult.Success.Voided(payment.Number));
        }

        return Result.Failure(PaymentResult.Errors.VoidFailed(response.Message));
    }

    #endregion Void

    #region Cancel

    /// <summary>
    /// Cancels a payment -- typically via a gateway cancel call.
    /// </summary>
    // Contract: post=payment.State==Void or Failed
    // Call: Gateway cancel action
    public static async Task<Result> CancelAsync(this Payment payment, IPaymentGatewayActionProvider gateway, CancellationToken cancellationToken = default)
    {
        var gatewayResult = await gateway.CancelAsync(payment.ResponseCode, payment, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult;

        var response = gatewayResult.Value;

        // Log: Record cancel response
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.State = PaymentState.Void;
            return Result.Ok(PaymentResult.Success.Voided(payment.Number));
        }

        payment.State = PaymentState.Failed;
        return Result.Failure(PaymentResult.Errors.CancelFailed(response.Message));
    }

    #endregion Cancel

    #region Credit

    /// <summary>
    /// Issues a credit (refund) against a completed payment via the gateway.
    /// </summary>
    // Contract: pre=payment.State==Completed && amount<=payment.Amount, post=creates credit/refund
    // Call: Gateway credit action
    public static async Task<Result> CreditAsync(this Payment payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!payment.CreditAllowed())
            return Result.Failure(PaymentResult.Errors.CreditNotAllowed);

        var gatewayResult = await gateway.CreditAsync(amount, payment.ResponseCode, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult;

        var response = gatewayResult.Value;

        // Log: Record credit response
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            return Result.Ok(PaymentResult.Success.Credited(payment.Number, amount));
        }

        return Result.Failure(PaymentResult.Errors.CreditFailed(response.Message));
    }

    #endregion Credit

    #region Helper Methods

    // Validate: Payment preconditions before gateway action -- mirrors Ruby handle_payment_preconditions
    // Enforce: Source required if payment method requires it; source must be supported
    private static Result HandlePaymentPreconditions(this Payment payment, IPaymentGatewayActionProvider gateway)
    {
        if (gateway.SourceRequired)
        {
            if (payment.SourceId is null || string.IsNullOrEmpty(payment.SourceType))
                return Result.Failure(PaymentResult.Errors.ProcessingSourceRequired);

            if (payment.State == PaymentState.Processing)
                return Result.Failure(PaymentResult.Errors.ProcessingAlreadyProcessing);
        }

        return Result.Ok();
    }

    // Enforce: Transition to Processing state before gateway action
    private static void StartedProcessing(this Payment payment)
    {
        if (payment.State == PaymentState.Checkout)
        {
            payment.State = PaymentState.Processing;
        }
    }

    // Call: Execute gateway action and handle response -- mirrors Ruby gateway_action
    private static async Task<Result> GatewayActionAsync(
        Payment payment,
        IPaymentGatewayActionProvider gateway,
        GatewayOptions options,
        Func<decimal, object?, GatewayOptions, CancellationToken, Task<Result<PaymentGatewayResponse>>> action,
        PaymentState successState,
        CancellationToken cancellationToken)
    {
        var source = payment.SourceType is not null ? new { Id = payment.SourceId, Type = payment.SourceType } : null;
        var gatewayResult = await action(payment.Amount, source, options, cancellationToken).ConfigureAwait(false);

        if (gatewayResult.IsFailure)
            return gatewayResult;

        var response = gatewayResult.Value;

        // Log: Record gateway response
        payment.RecordGatewayResponse(response);

        if (response.Success)
        {
            payment.ResponseCode = response.Authorization ?? payment.ResponseCode;
            payment.State = successState;
            return Result.Ok(successState == PaymentState.Pending
                ? PaymentResult.Success.Pended(payment.Number)
                : PaymentResult.Success.Completed(payment.Number));
        }

        payment.State = PaymentState.Failed;
        return Result.Failure(PaymentResult.Errors.GatewayError(response.Message));
    }

    // Call: Execute gateway action returning a typed result -- same as GatewayActionAsync
    private static Task<Result> GatewayActionInnerAsync(
        Payment payment,
        IPaymentGatewayActionProvider gateway,
        GatewayOptions options,
        Func<decimal, object?, GatewayOptions, CancellationToken, Task<Result<PaymentGatewayResponse>>> action,
        PaymentState successState,
        CancellationToken cancellationToken)
    {
        return GatewayActionAsync(payment, gateway, options, action, successState, cancellationToken);
    }

    // Log: Record gateway response details -- mirrors Ruby record_response
    private static void RecordGatewayResponse(this Payment payment, PaymentGatewayResponse response)
    {
        payment.AvsResponse = response.AvsResultCode;
        payment.CvvResponseCode = response.CvvResultCode;
        payment.CvvResponseMessage = response.CvvResultMessage;
    }

    #endregion Helper Methods
}