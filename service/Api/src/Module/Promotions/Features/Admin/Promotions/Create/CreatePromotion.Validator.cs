using FluentValidation;
using Module.Promotions.Features.Admin.Promotions.Shared.Validators;

namespace Module.Promotions.Features.Admin.Promotions.Create;

public static partial class CreatePromotion
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyPromotionParametersRules();
        }
    }
}
