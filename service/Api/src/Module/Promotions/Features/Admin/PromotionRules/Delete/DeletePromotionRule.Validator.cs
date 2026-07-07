namespace Module.Promotions.Features.Admin.PromotionRules.Delete;

public static partial class DeletePromotionRule
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PromotionId)
                .NotEmpty()
                .WithErrorCode("PromotionRule.PromotionId.Required")
                .WithMessage("Promotion ID is required.");

            RuleFor(x => x.RuleId)
                .NotEmpty()
                .WithErrorCode("PromotionRule.Id.Required")
                .WithMessage("Rule ID is required.");
        }
    }
}
