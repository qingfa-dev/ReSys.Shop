using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Services.Models;
using Module.Payment.Services.Abstractions;

namespace Module.Payment.Features.Shared.Commands;

public sealed record VoidOrderPaymentsCommand(Guid OrderId, string Reason) : ICommand;

public sealed class VoidOrderPaymentsCommandHandler(
    IApplicationDbContext dbContext,
    IGatewayRegistry gatewayRegistry,
    IPaymentProcessingService processingService)
    : ICommandHandler<VoidOrderPaymentsCommand>
{
    public async Task<Result> Handle(VoidOrderPaymentsCommand command, CancellationToken ct)
    {
        var payments = await dbContext.Set<PaymentCapture>()
            .Where(p => p.OrderId == command.OrderId)
            .ToListAsync(ct);

        foreach (var payment in payments)
        {
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure) continue;

            var options = new GatewayOptions
            {
                Email = string.Empty,
                Customer = string.Empty,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            await processingService.VoidTransactionAsync(payment, gatewayResult.Value, options, null, ct);
        }

        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
