using System.Data;
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
        var payments = await dbContext.Set<PaymentCapture>()
            .Where(p => p.OrderId == command.OrderId)
            .ToListAsync(ct);

        await using var transaction = await dbContext.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted, ct);

        try
        {
            foreach (var payment in payments)
            {
                var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
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

                var voidResult = await processingService.VoidTransactionAsync(
                    payment, gatewayResult.Value, options, null, ct);
                if (voidResult.IsFailure)
                {
                    await transaction.RollbackAsync(ct);
                    return voidResult.Errors;
                }
            }

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Ok();
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}