using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;

using Module.Payment.Services.Models;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Storefront.Payment.Shared.Mappings;

namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    public sealed record Command(Guid PaymentMethodId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Payment method must be active and not deleted
            var paymentMethod = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == command.PaymentMethodId && pm.Active && !pm.IsDeleted, cancellationToken);
            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Check: Gateway must be registered
            var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);
            var gateway = gatewayResult.Value;

            // Build: Metadata with payment method ID for gateway reference
            var metadata = new Dictionary<string, string>
            {
                [GatewayConstants.Metadata.PaymentMethodIdKey] = paymentMethod.Id.ToString()
            };

            // Call: Gateway setup intent — Stripe SetupIntent.Create for saved payment methods
            var setupResult = await gateway.CreateSetupIntentAsync(null, metadata, cancellationToken);
            if (setupResult.IsFailure) return setupResult.Errors;

            return setupResult.Value.MapToStoreDetail<Response>();
        }
    }
}