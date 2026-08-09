namespace Module.Identity.Features.Shared.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters).NotNull();
        }
    }
}