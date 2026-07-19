using Module.Profile.Features.Admin.Profiles.Shared.Models;

namespace Module.Profile.Features.Admin.Profiles.Get.PagedOrAll;
public static partial class GetUserProfilesPagedOrAll
{
    /// <summary>
    /// Represents the response contract for a list of profiles in paged results.
    /// </summary>
    public record Response : ProfileListItemResponse;
}