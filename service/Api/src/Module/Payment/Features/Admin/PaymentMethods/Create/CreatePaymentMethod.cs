using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Admin.PaymentMethods.Create;

public static partial class CreatePaymentMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (!gatewayRegistry.IsRegistered(request.ProviderKey))
                return PaymentMethodResult.Errors.ProviderNotRegistered(request.ProviderKey);

            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Errors;

            var method = createResult.Value;
            dbContext.Set<PaymentMethod>().Add(method);
            await dbContext.SaveChangesAsync(cancellationToken);

            return method.MapToDetail<Response>();
        }
    }
}
