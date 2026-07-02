using Module.Profile.Domain;

namespace Module.Profile.Features.Store.Profile.Get.Detail;

public static partial class GetProfile
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithErrorCode(UserProfileResult.Failure.NotFound.Code);
        }
    }
}
