using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Create;

public static partial class CreateShippingRate
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
                return createResult.Errors;

            var rate = createResult.Value;

            dbContext.Set<ShippingRate>().Add(rate);
            await dbContext.SaveChangesAsync(cancellationToken);

            return rate.MapToDetail<Response>();
        }
    }
}
