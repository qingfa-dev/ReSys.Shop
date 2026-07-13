using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Register;

public static partial class EmailRegister
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email).ApplyUserEmailRules();
            RuleFor(x => x.UserName).ApplyUsernameRules();
            RuleFor(x => x.Password).ApplyUserPasswordRules();
            RuleFor(x => x.FirstName).ApplyUserFirstNameRules();
            RuleFor(x => x.LastName).ApplyUserLastNameRules();
            RuleFor(x => x.Phone).ApplyUserPhoneRules()
                .When(m => !string.IsNullOrEmpty(m.Phone));

            RuleFor(x => x.AcceptTerm)
                .Equal(true)
                .WithErrorCode("Auth.Register.AcceptTerm.Required")
                .WithMessage("You must accept the terms and conditions.");
        }
    }
}