using Module.Profile.Features.Admin.Profiles.Shared.Models;

namespace Module.Profile.Features.Admin.Profiles.Shared.Validators;

public class ProfileRequestValidator : AbstractValidator<ProfileRequest>
{
    public ProfileRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}

public static class ProfileValidatorExtensions
{
    public static IRuleBuilderOptions<T, ProfileRequest> ApplyProfileRequestRules<T>(
        this IRuleBuilder<T, ProfileRequest> ruleBuilder)
    {
        return ruleBuilder.NotNull().SetValidator(new ProfileRequestValidator());
    }
}
