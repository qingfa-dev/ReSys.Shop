using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Features.Admin.CouponCodes.Shared.Mappings;

namespace Module.Promotions.Features.Admin.CouponCodes.Create;
/// <summary>Creates a new coupon code.</summary>
public static partial class CreateCouponCode
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Response>
    {
        /// <summary>Handles creating a coupon code.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created coupon code response.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Map the request to a new CouponCode entity.
            var result = request.MapToDomain();
            if (result.IsFailure)
                return result.Failures;

            var coupon = result.Value;

            // Persist: Save the new entity to the database.
            dbContext.Set<CouponCode>().Add(coupon);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return Result<Response>.Created(
                coupon.MapToDetail<Response>(),
                CouponCodeResult.Success.Created(coupon.Id));
        }
    }
}
