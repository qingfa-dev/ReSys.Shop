using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Adjustments;
using Module.Promotions.Domain.OrderPromotions;
using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Domain.Services;

namespace Module.UnitTests.Promotions.Domain.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionEvaluator")]
public class PromotionEvaluatorTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PromotionEvaluator _evaluator;

    public PromotionEvaluatorTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _evaluator = new PromotionEvaluator(_dbContext, NullLogger<PromotionEvaluator>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Order> CreateOrder(decimal itemTotal = 100, Guid? userId = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Number = "R" + Guid.NewGuid().ToString("N")[..8],
            UserId = userId ?? Guid.NewGuid(),
            ItemTotal = itemTotal,
            Total = itemTotal,
            Currency = "USD",
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private async Task<Promotion> CreatePromotion(
        string name = "Test Promo",
        bool active = true,
        PromotionKind kind = PromotionKind.Automatic,
        MatchPolicy matchPolicy = MatchPolicy.All,
        decimal? amountMin = null)
    {
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name,
            Active = active,
            Kind = kind,
            MatchPolicy = matchPolicy,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        if (amountMin.HasValue)
        {
            var ruleResult = PromotionRuleExtensions.Create(
                "ItemTotal",
                new Dictionary<string, string> { ["amount_min"] = amountMin.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                promotion.Id);
            if (ruleResult.IsSuccess)
                promotion.PromotionRules.Add(ruleResult.Value);
        }

        var actionResult = PromotionActionExtensions.Create(
            "CreateAdjustment",
            new Dictionary<string, string> { ["amount"] = "10", ["label"] = "Test Discount" },
            "FlatRate",
            promotion.Id);
        if (actionResult.IsSuccess)
            promotion.PromotionActions.Add(actionResult.Value);

        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return promotion;
    }

    [Fact(DisplayName = "Evaluate: Should create OrderPromotion and Adjustment when automatic promotion matches")]
    public async Task Evaluate_ShouldCreateOrderPromotionAndAdjustment_WhenAutomaticPromotionMatches()
    {
        var order = await CreateOrder(itemTotal: 150);
        await CreatePromotion(amountMin: 100);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().HaveCount(1);

        order.Adjustments.Should().Contain(a => a.SourceType == "PromotionAction");
        order.PromoTotal.Should().BeLessThan(0);
    }

    [Fact(DisplayName = "Evaluate: Should not match when automatic promotion fails rules")]
    public async Task Evaluate_ShouldNotMatch_WhenAutomaticPromotionFailsRules()
    {
        var order = await CreateOrder(itemTotal: 50);
        await CreatePromotion(amountMin: 100);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Evaluate: Should keep coupon promotion when already applied")]
    public async Task Evaluate_ShouldKeepCouponPromotion_WhenAlreadyApplied()
    {
        var order = await CreateOrder(itemTotal: 150);
        var promotion = await CreatePromotion(kind: PromotionKind.CouponCode, amountMin: 100);

        _dbContext.Set<OrderPromotion>().Add(new OrderPromotion
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PromotionId = promotion.Id,
            PromotionCodeId = Guid.NewGuid()
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Evaluate: Should remove stale automatic promotion")]
    public async Task Evaluate_ShouldRemoveStaleAutomaticPromotion()
    {
        var order = await CreateOrder(itemTotal: 50);
        var promotion = await CreatePromotion(amountMin: 100);

        _dbContext.Set<OrderPromotion>().Add(new OrderPromotion
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PromotionId = promotion.Id
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Evaluate: Should be idempotent on re-run")]
    public async Task Evaluate_ShouldBeIdempotent_OnReRun()
    {
        var order = await CreateOrder(itemTotal: 150);
        await CreatePromotion(amountMin: 100);

        var result1 = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);
        result1.IsSuccess.Should().BeTrue();

        var result2 = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate: Should apply multiple promotions")]
    public async Task Evaluate_ShouldApplyMultiplePromotions()
    {
        var order = await CreateOrder(itemTotal: 200);
        await CreatePromotion("Promo A", amountMin: 50);
        await CreatePromotion("Promo B", amountMin: 100);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Evaluate: Should return ok when no active promotions")]
    public async Task Evaluate_ShouldReturnOk_WhenNoActivePromotions()
    {
        var order = await CreateOrder();

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Evaluate: Should respect MatchPolicy All")]
    public async Task Evaluate_ShouldRespectMatchPolicy_All()
    {
        var order = await CreateOrder(itemTotal: 150);
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "All Policy",
            Active = true,
            Kind = PromotionKind.Automatic,
            MatchPolicy = MatchPolicy.All,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        promotion.PromotionRules.Add(new PromotionRule
        {
            Id = Guid.NewGuid(),
            Type = "ItemTotal",
            Preferences = new Dictionary<string, string> { ["amount_min"] = "100" },
            PromotionId = promotion.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        promotion.PromotionRules.Add(new PromotionRule
        {
            Id = Guid.NewGuid(),
            Type = "ItemTotal",
            Preferences = new Dictionary<string, string> { ["amount_min"] = "200" },
            PromotionId = promotion.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Evaluate: Should handle promotion with no rules and no actions")]
    public async Task Evaluate_ShouldHandlePromotionWithNoRulesAndActions()
    {
        var order = await CreateOrder();
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "Empty Promo",
            Active = true,
            Kind = PromotionKind.Automatic,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Evaluate: Should handle promotion with no rules but has actions")]
    public async Task Evaluate_ShouldHandlePromotionWithNoRulesButHasActions()
    {
        var order = await CreateOrder();
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "Actions Only",
            Active = true,
            Kind = PromotionKind.Automatic,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        promotion.PromotionActions.Add(new PromotionAction
        {
            Id = Guid.NewGuid(),
            Type = "FreeShipping",
            PromotionId = promotion.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _evaluator.Evaluate(order, order.UserId!.Value, [], TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var orderPromotions = await _dbContext.Set<OrderPromotion>().ToListAsync(TestContext.Current.CancellationToken);
        orderPromotions.Should().HaveCount(1);
    }
}
