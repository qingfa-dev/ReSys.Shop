using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Validators;

namespace Module.Promotions.Features.Admin.PromotionRules.Create;

public static partial class CreatePromotionRule
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.PromotionId)
                .NotEmpty()
                .WithErrorCode("PromotionRule.PromotionId.Required")
                .WithMessage("Promotion ID is required.");

            RuleFor(x => x.Request)
                .ApplyPromotionRuleParameterRules();
        }
    }
}
