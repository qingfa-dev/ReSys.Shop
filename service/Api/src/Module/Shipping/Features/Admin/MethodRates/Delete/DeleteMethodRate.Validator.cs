using FluentValidation;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.MethodRates.Delete;

public static partial class DeleteMethodRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RateId)
                .NotEmpty()
                .WithErrorCode(ShippingRateResult.Errors.NotFound(Guid.Empty).Code)
                .WithMessage("Rate ID is required.");
        }
    }
}
