using FluentValidation;

namespace Module.Promotions.Features.Admin.Promotions.Duplicate;

public static partial class DuplicatePromotion
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("Promotion.Id.Required")
                .WithMessage("Promotion ID is required.");
        }
    }
}
