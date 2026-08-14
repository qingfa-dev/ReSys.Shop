using Module.Billing.Features.Storefront.Payment.Shared.Mappings;

using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;
using Module.Ordering.Features.Storefront.AdvanceCheckoutState;
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Ordering.Features.Storefront.RecordOrderPaymentState;

using Module.Ordering.Domain.Orders;
using Module.Billing.Domain.PaymentCaptures;
using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;
using Module.Billing.Domain.PaymentMethods;
using GatewayOptions = Module.Billing.Services.Provider.GatewayOptions;
using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;

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
        IStockReservationService stockReservationService,
        ISender sender,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        // Contract: pre=orderId valid & user owns order, post=PaymentCapture persisted + gateway intent created
        /// <summary>Creates a payment intent for checkout.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Cart state must be Delivery (fresh) or Payment (re-pick / retry)
            var cartResult = await sender.Send(
                new GetCartForCheckoutQuery { CartId = command.Request.OrderId }, cancellationToken);
            if (cartResult.IsFailure) return cartResult.Errors;
            var cart = cartResult.Value;

            var currentState = cart.State;
            if (currentState is not (CheckoutState.PickDeliveryMethod or CheckoutState.PickPaymentMethod))
                return OrderResult.Errors.InvalidCheckoutTransition(currentState, CheckoutState.PickPaymentMethod);

            // Re-pick / retry: at Payment, void stale non-completed captures and release
            // prior reservations so a retry keeps a single reservation set and no orphans.
            if (currentState == CheckoutState.PickPaymentMethod)
            {
                var stale = await dbContext.Set<PaymentCapture>()
                    .Where(p => p.OrderId == command.Request.OrderId
                             && (p.State == PaymentRecordState.Checkout
                                 || p.State == PaymentRecordState.Processing
                                 || p.State == PaymentRecordState.Pending
                                 || p.State == PaymentRecordState.Failed))
                    .ToListAsync(cancellationToken);

                foreach (var p in stale)
                {
                    if (p.State == PaymentRecordState.Checkout)
                        p.Process();
                    if (p.State is PaymentRecordState.Processing or PaymentRecordState.Pending)
                        p.Void();
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await stockReservationService.ReleaseReservationsAsync(
                    cartToken: command.Request.OrderId.ToString(), ct: cancellationToken);

                CreatePaymentIntentLoggers.RetryVoidedStale(logger, stale.Count, command.Request.OrderId);
            }

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

            // Load: Active payment method — explicit id if provided, else first active
            var paymentMethod = command.Request.PaymentMethodId.HasValue
                ? await dbContext.Set<PaymentMethod>()
                    .FirstOrDefaultAsync(c => c.Id == command.Request.PaymentMethodId.Value && c.Active && !c.IsDeleted, cancellationToken)
                : await dbContext.Set<PaymentMethod>()
                    .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            var isOffline = GatewayConstants.Providers.IsOffline(paymentMethod.ProviderKey);

            // Create: PaymentCapture with no source — offline methods and Checkout Sessions
            // are both source-less; the gateway correlates via ResponseCode afterwards.
            var createResult = PaymentCaptureMethod.Create(
                amount: cart.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: command.Request.OrderId,
                sourceId: null,
                sourceType: null);
            if (createResult.IsFailure) return createResult.Errors;

            var payment = createResult.Value;
            payment.ProviderKey = paymentMethod.ProviderKey;
            dbContext.Set<Domain.PaymentCaptures.PaymentCapture>().Add(payment);

            if (isOffline)
            {
                // COD: transition straight to Pending — no gateway, no source.
                payment.Process();
                payment.Pend();

                CreatePaymentIntentLoggers.CodIntentCreated(logger, payment.Id);
            }
            else
            {
                var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
                if (gatewayResult.IsFailure)
                    return PaymentCaptureResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);
                var gateway = gatewayResult.Value;

                var options = new GatewayOptions
                {
                    Email = cart.Email ?? string.Empty,
                    Customer = cart.Email ?? string.Empty,
                    CustomerId = currentUser.UserId,
                    OrderId = $"{command.Request.OrderId}-{payment.Number}",
                    PaymentId = payment.Number,
                    IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                    StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
                    SuccessUrl = BuildSuccessUrl(command.Request.ReturnUrl, command.Request.OrderId),
                    CancelUrl = command.Request.CancelUrl,
                    Currency = string.IsNullOrWhiteSpace(command.Request.Currency)
                        ? GatewayConstants.Currency.Usd
                        : command.Request.Currency,
                };

                // Call: create hosted Checkout Session — no charge yet; webhook completes it.
                var sessionResult = await gateway.CreateCheckoutSessionAsync(cart.Total, options, cancellationToken);
                if (sessionResult.IsFailure)
                {
                    await stockReservationService.ReleaseReservationsAsync(
                        cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
                    return sessionResult.Errors;
                }

                payment.ResponseCode = sessionResult.Value.Authorization;
                payment.StripeSessionId = sessionResult.Value.Authorization;
                payment.CheckoutUrl = sessionResult.Value.CheckoutUrl;
                payment.Process();

                // Mirror: the payment now awaits the checkout webhook — stamp the order's processing time.
                await sender.Send(new RecordOrderPaymentStateCommand
                {
                    OrderId = command.Request.OrderId,
                    PaymentState = PaymentTimelineState.Processing,
                    AtUtc = DateTimeOffset.UtcNow
                }, cancellationToken);

                CreatePaymentIntentLoggers.SessionCreated(logger, payment.Id, sessionResult.Value.Authorization, sessionResult.Value.CheckoutUrl);
            }

            // Save: PaymentCapture to database
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Gateway session may have been created; it auto-expires in 24h. Release stock.
                await stockReservationService.ReleaseReservationsAsync(
                    cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
                throw;
            }

            // Advance: Cart state to PickPaymentMethod (records the selected payment method)
            await sender.Send(
                new AdvanceCheckoutStateCommand
                {
                    CartId = command.Request.OrderId,
                    TargetState = CheckoutState.PickPaymentMethod,
                    PaymentMethodId = paymentMethod.Id
                }, cancellationToken);

            // Map: Payment → storefront response DTO
            return payment.MapToStoreDetail<Response>();
        }
    }

    private static string? BuildSuccessUrl(string? returnUrl, Guid orderId)
        => string.IsNullOrWhiteSpace(returnUrl) ? null : $"{returnUrl}?order={orderId}";
}
