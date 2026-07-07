using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.OrderPromotions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.PromotionRules;

namespace Module.Promotions.Domain.Services;

public class PromotionEvaluator
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<PromotionEvaluator> _logger;

    public PromotionEvaluator(
        IApplicationDbContext dbContext,
        ILogger<PromotionEvaluator> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result> Evaluate(
        Order order,
        Guid userId,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken)
    {
        var userRolesList = userRoles.ToList();

        var activePromotions = await _dbContext.Set<Promotion>()
            .Include(p => p.PromotionRules)
            .Include(p => p.PromotionActions)
            .Where(p => p.Active && !p.IsDeleted
                && (p.StartsAtUtc == null || p.StartsAtUtc <= DateTimeOffset.UtcNow)
                && (p.ExpiresAtUtc == null || p.ExpiresAtUtc >= DateTimeOffset.UtcNow))
            .ToListAsync(cancellationToken);

        var existingOrderPromotions = await _dbContext.Set<OrderPromotion>()
            .Where(op => op.OrderId == order.Id)
            .ToListAsync(cancellationToken);

        var matchedPromotionIds = new HashSet<Guid>();

        foreach (var promotion in activePromotions)
        {
            if (promotion.Kind == PromotionKind.Automatic)
            {
                var rulesPass = EvaluateRules(promotion, order, userId, userRolesList);
                if (rulesPass)
                    matchedPromotionIds.Add(promotion.Id);
            }
        }

        foreach (var op in existingOrderPromotions)
        {
            if (op.PromotionCodeId is not null)
            {
                var promotion = activePromotions.FirstOrDefault(p => p.Id == op.PromotionId);
                if (promotion is not null)
                {
                    var rulesPass = EvaluateRules(promotion, order, userId, userRolesList);
                    if (rulesPass)
                        matchedPromotionIds.Add(promotion.Id);
                }
            }
        }

        var staleOrderPromotions = existingOrderPromotions
            .Where(op => !matchedPromotionIds.Contains(op.PromotionId)
                && (op.PromotionCodeId is null))
            .ToList();

        _dbContext.Set<OrderPromotion>().RemoveRange(staleOrderPromotions);

        var existingPromotionIds = existingOrderPromotions
            .Select(op => op.PromotionId)
            .ToHashSet();

        foreach (var promotionId in matchedPromotionIds)
        {
            if (!existingPromotionIds.Contains(promotionId))
            {
                var orderPromotion = new OrderPromotion
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PromotionId = promotionId,
                };
                _dbContext.Set<OrderPromotion>().Add(orderPromotion);
            }
        }

        var matchedActionIds = activePromotions
            .Where(p => matchedPromotionIds.Contains(p.Id))
            .SelectMany(p => p.PromotionActions)
            .Select(a => a.Id)
            .ToHashSet();

        var staleAdjustments = order.Adjustments
            .Where(a => a.SourceType == "PromotionAction"
                && !matchedActionIds.Contains(a.SourceId))
            .ToList();

        foreach (var adj in staleAdjustments)
        {
            order.Adjustments.Remove(adj);
        }

        foreach (var promotionId in matchedPromotionIds)
        {
            var promotion = activePromotions.First(p => p.Id == promotionId);

            foreach (var action in promotion.PromotionActions)
            {
                var hasAdjustment = order.Adjustments
                    .Any(a => a.SourceType == "PromotionAction" && a.SourceId == action.Id);

                if (!hasAdjustment)
                {
                    var result = ActionApplier.Apply(action, order);
                    if (result.IsSuccess)
                        order.Adjustments.Add(result.Value);
                }
            }
        }

        order.RecalculateTotals();
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "[PromotionEvaluator.Evaluated]: Order {OrderId} — {Matched} matched, {StalePromos} stale promotions removed",
                order.Id, matchedPromotionIds.Count, staleOrderPromotions.Count);
        }

        return Result.Ok();
    }

    private static bool EvaluateRules(
        Promotion promotion,
        Order order,
        Guid userId,
        List<string> userRoles)
    {
        if (promotion.PromotionRules.Count == 0)
            return promotion.PromotionActions.Count > 0;

        var results = promotion.PromotionRules
            .Select(r => RuleEngine.Evaluate(r, order, userId, userRoles))
            .ToList();

        return promotion.MatchPolicy switch
        {
            MatchPolicy.All => results.All(r => r),
            MatchPolicy.Any => results.Any(r => r),
            _ => false,
        };
    }
}
