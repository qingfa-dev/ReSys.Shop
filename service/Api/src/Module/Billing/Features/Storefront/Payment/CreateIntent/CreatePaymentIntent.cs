using Module.Billing.Features.Storefront.Payment.Shared.Mappings;

using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;
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
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IGatewayRegistry gatewayRegistry,
        IPaymentProcessingService processingService,
        IStockReservationService stockReservationService,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=orderId valid & user owns order, post=PaymentCapture persisted + gateway intent created
        /// <summary>Creates a payment intent for checkout.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Cart state must be Delivery
            var cartResult = await sender.Send(
                new GetCartForCheckoutQuery { CartId = command.Request.OrderId }, cancellationToken);
            if (cartResult.IsFailure) return cartResult.Errors;
            var cart = cartResult.Value;

            if (!Enum.TryParse<CheckoutState>(cart.State, out var currentState) || currentState != CheckoutState.Delivery)
                return OrderResult.Errors.InvalidCheckoutTransition(
                    Enum.TryParse<CheckoutState>(cart.State, out var s) ? s : CheckoutState.Address,
                    CheckoutState.Payment);

            // Reserve: Stock batched via Inventory service before gateway call
            foreach (var li in cart.LineItems)
            {
                var reserveResult = await stockReservationService.ReserveForVariantAsync(
                    variantId: li.VariantId,
                    quantity: li.Quantity,
                    cartToken: command.Request.OrderId.ToString(),
                    ttlMinutes: InventoryFeature.Storefront.StockReservations.TtlMinutesDefault,
                    ct: cancellationToken);

                if (reserveResult.IsFailure)
                {
                    // Compensate: release reservations already made in this loop
                    await stockReservationService.ReleaseReservationsAsync(
                        cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
                    return reserveResult.Errors;
                }
            }

            // Load: First active payment method
            var paymentMethod = command.Request.PaymentMethodId.HasValue
                ? await dbContext.Set<PaymentMethod>()
                    .FirstOrDefaultAsync(c => c.Id == command.Request.PaymentMethodId.Value && c.Active && !c.IsDeleted, cancellationToken)
                : await dbContext.Set<PaymentMethod>()
                    .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Create: PaymentCapture entity with order total, method, and order
            var createResult = Domain.PaymentCaptures.PaymentCaptureMethod.Create(
                amount: cart.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: command.Request.OrderId,
                sourceId: paymentMethod.ProviderKey == GatewayConstants.Providers.Bogus
                    ? command.Request.CardNumber
                    : command.Request.PaymentMethodToken,
                sourceType: paymentMethod.ProviderKey == GatewayConstants.Providers.Bogus
                    ? (command.Request.CardNumber is null ? null : GatewayConstants.SourceTypes.Card)
                    : (command.Request.PaymentMethodToken is null ? null : GatewayConstants.SourceTypes.PaymentMethod));
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
                // Replace with OrderNumber Generator
                OrderId = $"{command.Request.OrderId}-{payment.Number}",
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
                SuccessUrl = command.Request.ReturnUrl,
                Currency = string.IsNullOrWhiteSpace(command.Request.Currency)
                    ? GatewayConstants.Currency.Usd
                    : command.Request.Currency,
            };

            // Call: Gateway process (authorize or purchase depending on AutoCapture)
            var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
            if (processResult.IsFailure)
            {
                // Release reservations on gateway failure
                await stockReservationService.ReleaseReservationsAsync(
                    cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
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
                await stockReservationService.ReleaseReservationsAsync(
                    cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
                throw;
            }

            // Advance: Cart state to Payment
            await sender.Send(
                new AdvanceCheckoutStateCommand { CartId = command.Request.OrderId, TargetState = "Payment" }, cancellationToken);

            // Map: Payment → storefront response DTO
            return payment.MapToStoreDetail<Response>();
        }
    }
}
