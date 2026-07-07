using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Admin.ShippingMethods.Activate;

public static partial class ActivateShippingMethod
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            method.AvailableToUsers = true;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
