using Module.Payment.Features.Storefront.Payment.Shared.Mappings;

using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Domain.PaymentMethods;
using GatewayOptions = Module.Payment.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Payment.Services.Processing.IPaymentProcessingService;

using Module.Payment.Services.Provider;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

/// <summary>Creates a payment intent for checkout.</summary>
public static partial class CreatePaymentIntent
{
    public sealed record Command(Guid OrderId, Guid? PaymentMethodId = null, string? PaymentMethodToken = null, string? ReturnUrl = null, string? CardNumber = null, string? Currency = null) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IGatewayRegistry gatewayRegistry,
        IPaymentProcessingService processingService)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=orderId valid & user owns order, post=PaymentCapture persisted + gateway intent created
        /// <summary>Creates a payment intent for checkout.</summary>
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Current user must own the order
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.NotFound(command.OrderId);

            // Load: Order — verify ownership
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(x => x.Id == command.OrderId && x.UserId == userId, cancellationToken);
            if (order is null)
                return OrderResult.Errors.NotFound(command.OrderId);

            // Load: First active payment method
            var paymentMethod = command.PaymentMethodId.HasValue
                ? await dbContext.Set<PaymentMethod>()
                    .FirstOrDefaultAsync(c => c.Id == command.PaymentMethodId.Value && c.Active && !c.IsDeleted, cancellationToken)
                : await dbContext.Set<PaymentMethod>()
                    .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Create: PaymentCapture entity with order total, method, and order
            var createResult = Domain.PaymentCaptures.PaymentCaptureMethod.Create(
                amount: order.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: order.Id,
                sourceId: paymentMethod.ProviderKey == GatewayConstants.Providers.Bogus
                    ? command.CardNumber
                    : command.PaymentMethodToken,
                sourceType: paymentMethod.ProviderKey == GatewayConstants.Providers.Bogus
                    ? (command.CardNumber is null ? null : GatewayConstants.SourceTypes.Card)
                    : (command.PaymentMethodToken is null ? null : GatewayConstants.SourceTypes.PaymentMethod));
            if (createResult.IsFailure) return createResult.Errors;

            var payment = createResult.Value;
            dbContext.Set<PaymentCapture>().Add(payment);

            // Check: Gateway must be registered
            var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);
            var gateway = gatewayResult.Value;

            // Build: Gateway options with order and payment identifiers
            var options = new GatewayOptions
            {
                Email = order.Email ?? string.Empty,
                Customer = order.Email ?? string.Empty,
                CustomerId = currentUser.UserId,
                OrderId = $"{order.Number}-{payment.Number}",
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
                SuccessUrl = command.ReturnUrl,
                Currency = command.Currency ?? GatewayConstants.Currency.Usd,
            };

            // Call: Gateway process (authorize or purchase depending on AutoCapture)
            var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
            if (processResult.IsFailure) return processResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Payment → storefront response DTO
            return payment.MapToStoreDetail<Response>();
        }
    }
}
