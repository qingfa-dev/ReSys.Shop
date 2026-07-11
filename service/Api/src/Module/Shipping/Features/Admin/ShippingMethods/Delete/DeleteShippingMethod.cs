using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.ShippingMethods.Delete;

public static partial class DeleteShippingMethod
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

            var hasRates = await dbContext.Set<ShippingRate>()
                .AnyAsync(r => r.ShippingMethodId == command.Id, cancellationToken);

            if (hasRates)
                return ShippingMethodResult.Failure.HasAssociatedRates;

            method.IsDeleted = true;
            method.DeletedAtUtc = DateTimeOffset.UtcNow;
            method.DeletedBy = "System";

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
