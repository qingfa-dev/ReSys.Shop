using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.GetByIsoCode;

public static partial class GetStateByIso
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(expression: x => x.IsoCode)
                .NotEmpty()
                .WithErrorCode(errorCode: StateResult.Errors.AbbreviationRequired.Code)
                .WithMessage(errorMessage: StateResult.Errors.AbbreviationRequired.Message)
                .MaximumLength(maximumLength: StateConstant.Constraints.MaxAbbreviationLength)
                .WithErrorCode(errorCode: StateResult.Errors.AbbreviationTooLong.Code)
                .WithMessage(errorMessage: StateResult.Errors.AbbreviationTooLong.Message);
        }
    }
}
