using Microsoft.EntityFrameworkCore;
using Module.Promotions.Domain.CouponCodes;

namespace Module.Promotions.Features.Admin.CouponCodes.Delete;
/// <summary>Cancels (soft-deletes) a coupon code by ID.</summary>
public static partial class DeleteCouponCode
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command>
    {
        /// <summary>Handles canceling a coupon code.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Verify the coupon code exists.
            var couponCode = await dbContext.Set<CouponCode>().FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
            if (couponCode is null)
                return CouponCodeResult.Errors.NotFound(command.Id);

            // Update: Cancel the coupon code.
            var result = couponCode.Cancel();
            if (result.IsFailure)
                return result.Failures;

            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
