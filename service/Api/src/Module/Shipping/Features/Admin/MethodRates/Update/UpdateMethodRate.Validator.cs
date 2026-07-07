using FluentValidation;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Features.Admin.MethodRates.Update;

public static partial class UpdateMethodRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RateId)
                .NotEmpty()
                .WithErrorCode(ShippingRateResult.Errors.NotFound(Guid.Empty).Code)
                .WithMessage("Rate ID is required.");

            // Validate: Weight range when both specified
            RuleFor(x => x.Request)
                .Must(r => !r.MinWeight.HasValue || !r.MaxWeight.HasValue || r.MinWeight.Value <= r.MaxWeight.Value)
                .WithErrorCode(ShippingRateResult.Errors.MinWeightExceedsMaxWeight.Code)
                .WithMessage(ShippingRateResult.Errors.MinWeightExceedsMaxWeight.Description)
                .When(x => x.Request.MinWeight.HasValue && x.Request.MaxWeight.HasValue);

            RuleFor(x => x.Request.MinWeight)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.MinWeight.HasValue)
                .WithErrorCode(ShippingRateResult.Errors.WeightNegative.Code)
                .WithMessage(ShippingRateResult.Errors.WeightNegative.Description);

            RuleFor(x => x.Request.MaxWeight)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.MaxWeight.HasValue)
                .WithErrorCode(ShippingRateResult.Errors.WeightNegative.Code)
                .WithMessage(ShippingRateResult.Errors.WeightNegative.Description);
        }
    }
}
