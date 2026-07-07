using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.PromotionActions.Shared.Validators;

namespace Module.Promotions.Features.Admin.PromotionActions.Create;

public static partial class CreatePromotionAction
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.PromotionId)
                .NotEmpty()
                .WithErrorCode("PromotionAction.PromotionId.Required")
                .WithMessage("Promotion ID is required.");

            RuleFor(x => x.Request)
                .ApplyPromotionActionParameterRules();
        }
    }
}
