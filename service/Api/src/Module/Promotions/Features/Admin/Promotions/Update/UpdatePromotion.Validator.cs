using FluentValidation;
using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Features.Admin.Promotions.Update;

public static partial class UpdatePromotion
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            When(x => x.Request.Name is not null, () =>
            {
                RuleFor(x => x.Request.Name).ApplyNameRules();
            });

            When(x => x.Request.Code is not null, () =>
            {
                RuleFor(x => x.Request.Code).ApplyCodeRules();
            });

            When(x => x.Request.Description is not null, () =>
            {
                RuleFor(x => x.Request.Description).ApplyDescriptionRules();
            });

            When(x => x.Request.Path is not null, () =>
            {
                RuleFor(x => x.Request.Path).ApplyPathRules();
            });

            When(x => x.Request.MatchPolicy.HasValue, () =>
            {
                RuleFor(x => (MatchPolicy)x.Request.MatchPolicy!).ApplyMatchPolicyRules();
            });

            When(x => x.Request.Kind.HasValue, () =>
            {
                RuleFor(x => (PromotionKind)x.Request.Kind!).ApplyKindRules();
            });
        }
    }
}
