namespace Module.Identity.Features.Shared.Admin.Roles.Get.ById;

public static partial class GetRoleById
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithErrorCode("RoleIdRequired");
        }
    }
}