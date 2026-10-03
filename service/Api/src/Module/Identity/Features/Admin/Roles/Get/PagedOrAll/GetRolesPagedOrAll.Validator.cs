namespace Module.Identity.Features.Shared.Admin.Roles.Get.PagedOrAll;

public static partial class GetRolesPagedOrAll
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters).NotNull();
        }
    }
}