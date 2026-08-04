using Module.Profile.Features.Shared.Profiles.Models;

namespace Module.Profile.Features.Admin.Profiles.Get.PagedOrAll;
public static partial class GetUserProfilesPagedOrAll
{
    /// <summary>
    /// Represents the response contract for a list of profiles in paged results.
    /// </summary>
    public record Response : ProfileListItemResponse;
}