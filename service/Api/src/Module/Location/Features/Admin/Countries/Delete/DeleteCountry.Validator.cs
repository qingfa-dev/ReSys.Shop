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
                .WithErrorCode(errorCode: CountryResult.Errors.IdRequired.Code)
                .WithMessage(errorMessage: CountryResult.Errors.IdRequired.Message);
        }
    }
}
