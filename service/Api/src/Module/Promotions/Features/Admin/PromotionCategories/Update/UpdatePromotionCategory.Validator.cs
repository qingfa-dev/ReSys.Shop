using FluentValidation;
using Module.Promotions.Domain.PromotionCategories;

namespace Module.Promotions.Features.Admin.PromotionCategories.Update;

public static partial class UpdatePromotionCategory
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            When(x => x.Request.Name is not null, () =>
            {
                RuleFor(x => x.Request.Name!).ApplyNameRules();
            });
        }
    }
}
