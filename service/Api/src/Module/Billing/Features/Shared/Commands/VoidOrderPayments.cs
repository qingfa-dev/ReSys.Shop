using System.Data;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Provider;

using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;

namespace Module.Billing.Features.Shared.Commands;

public sealed record VoidOrderPaymentsCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = default!;
}

/// <summary>Handles voiding all pending payments for an order within a single transaction — rolls back on any failure.</summary>
// Contract: pre=OrderId valid, post=All non-void payments for order are voided || Result.IsFailure
public sealed class VoidOrderPaymentsCommandHandler(
    IApplicationDbContext dbContext,
    IGatewayRegistry gatewayRegistry,
    IPaymentProcessingService processingService)
    : ICommandHandler<VoidOrderPaymentsCommand>
{
    /// <summary>Voids all non-completed payments for the specified order within a transaction scope.</summary>
    /// <param name="command">The command containing the order ID and reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A success result or the first failure encountered.</returns>
    public async Task<Result> Handle(VoidOrderPaymentsCommand command, CancellationToken ct)
    {
        // Load: Fetch all voidable payments associated with the order. Completed,
        // Failed, Disputed and Invalid captures are not voidable (refund instead),
        // and calling the gateway on them would fail the whole transaction.
        var payments = await dbContext.Set<PaymentCapture>()
            .Where(p => p.OrderId == command.OrderId
                     && p.State != PaymentRecordState.Completed
                     && p.State != PaymentRecordState.Failed
                     && p.State != PaymentRecordState.Disputed
                     && p.State != PaymentRecordState.Invalid)
            .ToListAsync(ct);

        // Await: Begin transaction for atomic void operation
        await using var transaction = await dbContext.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted, ct);

        foreach (var payment in payments)
        {
            // Call: Resolve gateway provider for this payment method
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            // Check: Fail-fast if no gateway is registered for the provider
            if (gatewayResult.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            }

            var options = new GatewayOptions
            {
                Email = string.Empty,
                Customer = string.Empty,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            // Call: Void the payment through the gateway processing service
            var voidResult = await processingService.VoidTransactionAsync(
                payment, gatewayResult.Value, options, null, ct);
            // Check: Roll back the entire transaction if any single void fails
            if (voidResult.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return voidResult.Errors;
            }
        }

        // Await: Persist changes and commit transaction
        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result.Ok();
    }
}
