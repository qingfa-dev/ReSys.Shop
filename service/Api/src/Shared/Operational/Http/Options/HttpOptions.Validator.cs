using FluentValidation;

namespace Shared.Operational.Http.Options;

public sealed class HttpOptionsValidator : AbstractValidator<HttpOptions>
{
    public HttpOptionsValidator()
    {
        RuleFor(x => x.DefaultTimeoutSeconds)
            .InclusiveBetween(
                HttpConstant.Constraints.DefaultTimeoutSecondsMin,
                HttpConstant.Constraints.DefaultTimeoutSecondsMax)
            .WithErrorCode(HttpOptionsResult.Failure.DefaultTimeoutSecondsOutOfRange.Code)
            .WithMessage(HttpOptionsResult.Failure.DefaultTimeoutSecondsOutOfRange.Message);

        RuleForEach(x => x.Clients).ChildRules(client =>
        {
            client.RuleFor(x => x.Value.BaseAddress)
                .NotEmpty()
                .WithErrorCode(HttpOptionsResult.Failure.ClientBaseAddressEmpty.Code)
                .WithMessage(HttpOptionsResult.Failure.ClientBaseAddressEmpty.Message)
                .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
                .WithErrorCode(HttpOptionsResult.Failure.ClientBaseAddressInvalid.Code)
                .WithMessage(HttpOptionsResult.Failure.ClientBaseAddressInvalid.Message);

            client.RuleFor(x => x.Value.TimeoutSeconds)
                .GreaterThanOrEqualTo(HttpConstant.Constraints.TimeoutSecondsMin)
                .WithErrorCode(HttpOptionsResult.Failure.ClientTimeoutSecondsNegative.Code)
                .WithMessage(HttpOptionsResult.Failure.ClientTimeoutSecondsNegative.Message);
        });
    }
}
