using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.GetById;

public static partial class GetUserById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithErrorCode(UserResult.Failure.IdRequired.Code);
        }
    }
}