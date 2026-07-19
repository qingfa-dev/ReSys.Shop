using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.Models;

using GatewayOptions = Module.Payment.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Payment.Services.Processing.IPaymentProcessingService;

namespace Module.Payment.Features.Shared.Commands;

public sealed record VoidOrderPaymentsCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = default!;
}

public sealed class VoidOrderPaymentsCommandHandler(
    IApplicationDbContext dbContext,
    IGatewayRegistry gatewayRegistry,
    IPaymentProcessingService processingService)
    : ICommandHandler<VoidOrderPaymentsCommand>
{
    // Contract: pre=OrderId valid, post=All non-void payments for order are voided || Result.IsFailure
    public async Task<Result> Handle(VoidOrderPaymentsCommand command, CancellationToken ct)
    {
        // Load: All payment captures for the given order
        var payments = await dbContext.Set<PaymentCapture>()
            .Where(p => p.OrderId == command.OrderId)
            .ToListAsync(ct);

        // Batch: Void each payment through its registered gateway
        foreach (var payment in payments)
        {
            // Check: Skip if no gateway registered for this payment's provider
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure) continue;

            // Build: Gateway options with idempotency key
            var options = new GatewayOptions
            {
                Email = string.Empty,
                Customer = string.Empty,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            // Call: Gateway void — if it fails, abort the batch
            var voidResult = await processingService.VoidTransactionAsync(payment, gatewayResult.Value, options, null, ct);
            if (voidResult.IsFailure)
                return voidResult.Errors;
        }

        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }
}