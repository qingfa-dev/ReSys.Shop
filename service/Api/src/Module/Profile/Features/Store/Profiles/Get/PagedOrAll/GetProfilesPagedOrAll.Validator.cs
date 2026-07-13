namespace Module.Profile.Features.Store.Profiles.Get.PagedOrAll;

public static partial class GetProfilesPagedOrAll
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters).NotNull();
        }
    }
}