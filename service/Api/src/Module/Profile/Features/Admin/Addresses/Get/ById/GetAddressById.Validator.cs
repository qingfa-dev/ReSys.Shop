namespace Module.Profile.Features.Admin.Addresses.Get.ById;

public static partial class GetAddressById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
