using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;

using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Admin.Shared.Mappings;

namespace Module.Billing.Features.Admin.PaymentMethods.Create;

/// <summary>Creates a new payment method.</summary>
public static partial class CreatePaymentMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Creates a new payment method.</summary>
        // Contract: pre=command!=null, post=method!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Provider must be registered before creating the method
            if (!gatewayRegistry.IsRegistered(request.ProviderKey))
                return PaymentMethodResult.Errors.ProviderNotRegistered(request.ProviderKey);

            // Map: Request → PaymentMethod domain entity
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Errors;

            var method = createResult.Value;

            // Create: Persist new payment method to database
            dbContext.Set<PaymentMethod>().Add(method);
            // Await: Commit the transaction
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: PaymentMethod → response DTO
            return method.MapToDetail<Response>();
        }
    }
}