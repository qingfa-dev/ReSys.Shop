using Module.Payment.Services.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Domain.PaymentMethods;

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
            var paymentMethod = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == command.PaymentMethodId && pm.Active && !pm.IsDeleted, cancellationToken);
            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);
            var gateway = gatewayResult.Value;

            var metadata = new Dictionary<string, string>
            {
                [GatewayConstants.Metadata.PaymentMethodIdKey] = paymentMethod.Id.ToString()
            };

            var setupResult = await gateway.CreateSetupIntentAsync(null, metadata, cancellationToken);
            if (setupResult.IsFailure) return setupResult.Errors;

            return new Response { ClientSecret = setupResult.Value.SetupIntentClientSecret! };
        }
    }
}
