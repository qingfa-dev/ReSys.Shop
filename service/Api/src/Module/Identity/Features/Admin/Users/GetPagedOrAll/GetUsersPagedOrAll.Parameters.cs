using Shared.Operational.Persistence.Specifications.Querying;

namespace Module.Identity.Features.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    /// <summary>
    /// Represents the parameters for querying users, including pagination and filtering options.
    /// </summary>
    public record Parameters : QueryingParameters;
}
