namespace Module.Profile.Features.Admin.Profiles.GetUserProfile;

public static partial class GetUserProfile
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
