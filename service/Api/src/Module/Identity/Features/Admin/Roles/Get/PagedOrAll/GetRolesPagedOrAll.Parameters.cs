using Shared.Operational.Persistence.Specifications.Querying;

namespace Module.Identity.Features.Admin.Roles.Get.PagedOrAll;

public static partial class GetRolesPagedOrAll
{
    /// <summary>
    /// Represents the parameters for querying roles, including pagination and filtering options.
    /// </summary>
    public record Parameters : QueryingParameters;
}
