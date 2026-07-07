using FluentValidation;
using Module.Promotions.Features.Admin.PromotionCategories.Shared.Validators;

namespace Module.Promotions.Features.Admin.PromotionCategories.Create;

public static partial class CreatePromotionCategory
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyPromotionCategoryParametersRules();
        }
    }
}
