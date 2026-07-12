using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Domain.PaymentMethods;
using GatewayOptions = Module.Payment.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Payment.Services.Processing.IPaymentProcessingService;

using Module.Payment.Services.Models;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public sealed record Command(Guid OrderId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IGatewayRegistry gatewayRegistry,
        IPaymentProcessingService processingService)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Failure.NotFound(command.OrderId);

            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(x => x.Id == command.OrderId && x.UserId == userId, cancellationToken);
            if (order is null)
                return OrderResult.Failure.NotFound(command.OrderId);

            var paymentMethod = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            var createResult = Domain.PaymentCaptures.PaymentCaptureMethod.Create(
                amount: order.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: order.Id);
            if (createResult.IsFailure) return createResult.Errors;

            var payment = createResult.Value;
            dbContext.Set<PaymentCapture>().Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);
            var gateway = gatewayResult.Value;

            var options = new GatewayOptions
            {
                Email = order.Email ?? string.Empty,
                Customer = order.Email ?? string.Empty,
                CustomerId = currentUser.UserId,
                OrderId = $"{order.Number}-{payment.Number}",
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
            if (processResult.IsFailure) return processResult.Errors;
            // IntentClientSecret set by RecordGatewayResponse in PaymentProcessingService during ProcessAsync

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Currency = order.Currency,
                OrderId = payment.OrderId,
                PaymentMethodId = payment.PaymentMethodId,
                State = payment.State.ToString(),
                ClientSecret = payment.IntentClientSecret,
                CreatedAtUtc = payment.CreatedAtUtc,
                ModifiedAtUtc = payment.ModifiedAtUtc,
            };
        }
    }
}
