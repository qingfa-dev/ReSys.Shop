using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;

using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Admin.PaymentMethods.Update;

public static partial class UpdatePaymentMethod
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return PaymentMethodResult.Errors.NotFound;

            if (command.Request.ProviderKey is not null && !gatewayRegistry.IsRegistered(command.Request.ProviderKey))
                return PaymentMethodResult.Errors.ProviderNotRegistered(command.Request.ProviderKey);

            var result = command.Request.MapUpdateToDomain(method);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return method.MapToDetail<Response>();
        }
    }
}
