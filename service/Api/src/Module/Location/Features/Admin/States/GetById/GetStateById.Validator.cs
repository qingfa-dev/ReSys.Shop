using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.GetById;

public static partial class GetStateById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(expression: x => x.Id)
                .NotEmpty()
                .WithErrorCode(errorCode: StateResult.Failure.IdRequired.Code)
                .WithMessage(errorMessage: StateResult.Failure.IdRequired.Message);
        }
    }
}
