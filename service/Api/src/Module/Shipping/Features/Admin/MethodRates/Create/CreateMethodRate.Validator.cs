using FluentValidation;
using Module.Shipping.Features.Admin.MethodRates.Shared.Validators;

namespace Module.Shipping.Features.Admin.MethodRates.Create;

public static partial class CreateMethodRate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyMethodRateParametersRules()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Request).ApplyMethodRateWeightRules();
                });
        }
    }
}
