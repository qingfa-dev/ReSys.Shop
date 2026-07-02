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
                .WithErrorCode(errorCode: StateResult.Failure.AbbreviationRequired.Code)
                .WithMessage(errorMessage: StateResult.Failure.AbbreviationRequired.Message)
                .MaximumLength(maximumLength: StateConstant.Constraints.MaxAbbreviationLength)
                .WithErrorCode(errorCode: StateResult.Failure.AbbreviationTooLong.Code)
                .WithMessage(errorMessage: StateResult.Failure.AbbreviationTooLong.Message);
        }
    }
}
