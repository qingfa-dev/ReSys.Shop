using Module.Billing.Features.Storefront.Payment.Shared.Mappings;

using Module.Inventory.Features.Storefront.ReleaseCartStockReservations;
using Module.Inventory.Features.Storefront.ReserveCartStock;
using Module.Inventory.Services.Abstractions;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;
using Module.Ordering.Features.Storefront.GetCartForCheckout;

using Module.Ordering.Domain.Orders;
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Domain.PaymentMethods;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Billing.Services.Processing.IPaymentProcessingService;

using Module.Billing.Services.Provider;

namespace Module.Billing.Features.Storefront.Payment.CreateIntent;

/// <summary>Creates a payment intent for checkout.</summary>
public static partial class CreatePaymentIntent
{
    public sealed record Command(Guid OrderId, Guid? PaymentMethodId = null, string? PaymentMethodToken = null, string? ReturnUrl = null, string? CardNumber = null, string? Currency = null) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IGatewayRegistry gatewayRegistry,
        IPaymentProcessingService processingService,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=orderId valid & user owns order, post=PaymentCapture persisted + gateway intent created
        /// <summary>Creates a payment intent for checkout.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Cart state must be Delivery
            var cartResult = await sender.Send(
                new GetCartForCheckoutQuery { CartId = command.OrderId }, cancellationToken);
            if (cartResult.IsFailure) return cartResult.Errors;
            var cart = cartResult.Value;

            if (!Enum.TryParse<CheckoutState>(cart.State, out var currentState) || currentState != CheckoutState.Delivery)
                return OrderResult.Errors.InvalidCheckoutTransition(
                    Enum.TryParse<CheckoutState>(cart.State, out var s) ? s : CheckoutState.Address,
                    CheckoutState.Payment);

            // Reserve: Stock atomically before gateway call
            var reserveResult = await sender.Send(
                new ReserveCartStockCommand
                {
                    CartId = command.OrderId,
                    LineItems = cart.LineItems.Select(li => new ReserveLineItem
                    {
                        VariantId = li.VariantId,
                        Quantity = li.Quantity
                    }).ToList()
                }, cancellationToken);
            if (reserveResult.IsFailure) return reserveResult.Errors;

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
                amount: cart.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: command.OrderId,
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
                Email = cart.Email ?? string.Empty,
                Customer = cart.Email ?? string.Empty,
                CustomerId = currentUser.UserId,
                OrderId = $"{command.OrderId}-{payment.Number}",
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
                SuccessUrl = command.ReturnUrl,
                Currency = command.Currency ?? GatewayConstants.Currency.Usd,
            };

            // Call: Gateway process (authorize or purchase depending on AutoCapture)
            var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
            if (processResult.IsFailure)
            {
                // Release reservations on gateway failure
                await sender.Send(
                    new ReleaseCartStockReservationsCommand { CartId = command.OrderId }, CancellationToken.None);
                return processResult.Errors;
            }

            // Save: PaymentCapture to database
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // E3: Gateway succeeded but save failed — void payment and release reservations
                await processingService.VoidAsync(payment, gateway, options, CancellationToken.None);
                await sender.Send(
                    new ReleaseCartStockReservationsCommand { CartId = command.OrderId }, CancellationToken.None);
                throw;
            }

            // Advance: Cart state to Payment
            await sender.Send(
                new AdvanceCheckoutStateCommand { CartId = command.OrderId, TargetState = "Payment" }, cancellationToken);

            // Map: Payment → storefront response DTO
            return payment.MapToStoreDetail<Response>();
        }
    }
}
