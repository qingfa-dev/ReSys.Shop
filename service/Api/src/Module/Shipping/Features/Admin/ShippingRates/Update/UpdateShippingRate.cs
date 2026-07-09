using Module.Shipping.Domain.ShippingRates;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingRates.Update;

public static partial class UpdateShippingRate
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var rate = await dbContext.Set<ShippingRate>()
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (rate is null)
                return ShippingRateResult.Errors.NotFound(command.Id);

            var result = command.Request.MapUpdateToDomain(rate);
            if (result.IsFailure)
                return result.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return rate.MapToDetail<Response>();
        }
    }
}
