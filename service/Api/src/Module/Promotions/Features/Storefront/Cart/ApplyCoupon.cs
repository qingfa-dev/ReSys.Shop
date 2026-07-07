using Microsoft.EntityFrameworkCore;

using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.CouponCodes;
using Module.Promotions.Domain.OrderPromotions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.Services;

namespace Module.Promotions.Features.Storefront.Cart;

public static partial class ApplyCoupon
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var couponCode = await dbContext.Set<CouponCode>()
                .Include(c => c.Promotion)
                .ThenInclude(p => p.PromotionRules)
                .Include(c => c.Promotion)
                .ThenInclude(p => p.PromotionActions)
                .Where(c => c.Code == command.Request.Code && c.State == CouponCodeState.Active)
                .FirstOrDefaultAsync(cancellationToken);

            if (couponCode is null)
                return CouponCodeResult.Errors.NotFoundByCode;

            var promotion = couponCode.Promotion;
            if (promotion is null || !promotion.IsActive())
                return PromotionResult.Errors.Inactive;

            var existingOrderPromotion = await dbContext.Set<OrderPromotion>()
                .AnyAsync(op => op.OrderId == cart.Id && op.PromotionCodeId == couponCode.Id, cancellationToken);

            if (existingOrderPromotion)
                return CouponCodeResult.Errors.AlreadyAppliedToOrder;

            if (promotion.PromotionRules.Count > 0)
            {
                var rulesPass = promotion.PromotionRules
                    .Select(r => RuleEngine.Evaluate(r, cart, userId, []))
                    .ToList();

                var allPass = promotion.MatchPolicy switch
                {
                    MatchPolicy.All => rulesPass.All(r => r),
                    MatchPolicy.Any => rulesPass.Any(r => r),
                    _ => false,
                };

                if (!allPass)
                    return CouponCodeResult.Errors.RulesNotMet;
            }

            var orderPromotion = new OrderPromotion
            {
                Id = Guid.NewGuid(),
                OrderId = cart.Id,
                PromotionId = promotion.Id,
                PromotionCodeId = couponCode.Id
            };
            dbContext.Set<OrderPromotion>().Add(orderPromotion);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
