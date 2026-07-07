using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingRates.Delete;

public static partial class DeleteShippingRate
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var rate = await dbContext.Set<ShippingRate>()
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (rate is null)
                return ShippingRateResult.Errors.NotFound(command.Id);

            dbContext.Set<ShippingRate>().Remove(rate);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
