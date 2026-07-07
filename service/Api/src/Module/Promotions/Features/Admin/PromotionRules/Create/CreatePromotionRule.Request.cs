using Module.Promotions.Features.Admin.PromotionRules.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionRules.Create;

public static partial class CreatePromotionRule
{
    public class Request : PromotionRuleRequest
    {
        public required Guid PromotionId { get; init; }
    }
}
