namespace Module.Promotions.Features.Admin.PromotionActions.Delete;

public static partial class DeletePromotionAction
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PromotionId)
                .NotEmpty()
                .WithErrorCode("PromotionAction.PromotionId.Required")
                .WithMessage("Promotion ID is required.");

            RuleFor(x => x.ActionId)
                .NotEmpty()
                .WithErrorCode("PromotionAction.Id.Required")
                .WithMessage("Action ID is required.");
        }
    }
}
