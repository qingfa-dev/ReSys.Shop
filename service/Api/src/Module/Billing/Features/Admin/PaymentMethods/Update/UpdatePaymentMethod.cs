using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;

using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Billing.Features.Admin.PaymentMethods.Update;

/// <summary>Updates an existing payment method's details.</summary>
public static partial class UpdatePaymentMethod
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates an existing payment method's details.</summary>
        // Contract: pre=command!=null && method exists, post=method updated
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Payment method by ID
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            // Check: Payment method must exist
            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            // Check: Provider must be registered if changing provider
            if (command.Request.ProviderKey is not null && !gatewayRegistry.IsRegistered(command.Request.ProviderKey))
                return PaymentMethodResult.Errors.ProviderNotRegistered(command.Request.ProviderKey);

            // Update: Apply request fields to existing entity
            var result = command.Request.MapUpdateToDomain(method);
            if (result.IsFailure)
                return result.Errors;

            // Await: Persist changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: PaymentMethod → response DTO
            return method.MapToDetail<Response>();
        }
    }
}