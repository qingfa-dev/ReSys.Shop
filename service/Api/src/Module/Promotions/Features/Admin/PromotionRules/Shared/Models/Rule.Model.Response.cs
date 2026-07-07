namespace Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

public class PromotionRuleDetailResponse : PromotionRuleParameters
{
    public Guid Id { get; init; }
    public Guid PromotionId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public class PromotionRuleListResponse : PromotionRuleParameters
{
    public Guid Id { get; init; }
}
