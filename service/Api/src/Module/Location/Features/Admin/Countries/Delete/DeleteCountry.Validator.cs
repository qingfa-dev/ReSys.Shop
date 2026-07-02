using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.Delete;

public static partial class DeleteCountry
{
    // ============ COMMAND VALIDATOR ============
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(expression: x => x.Id)
                .NotEmpty()
                .WithErrorCode(errorCode: CountryResult.Failure.IdRequired.Code)
                .WithMessage(errorMessage: CountryResult.Failure.IdRequired.Message);
        }
    }
}
