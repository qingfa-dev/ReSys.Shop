using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.Delete;

public static partial class DeleteState
{
    public sealed class Validator : AbstractValidator<Command>
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