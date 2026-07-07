using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Create;

public static partial class CreateShippingMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Failures;

            var method = createResult.Value;

            dbContext.Set<ShippingMethod>().Add(method);
            await dbContext.SaveChangesAsync(cancellationToken);

            return method.MapToDetail<Response>();
        }
    }
}
