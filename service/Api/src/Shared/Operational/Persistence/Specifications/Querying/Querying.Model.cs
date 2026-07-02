using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.Operational.Persistence.Specifications.Querying;

/// <summary>
/// A fully parsed, validated record carrying all four querying concern models.
/// Produced by <see cref="QueryingParametersExtensions.ParseAll"/>.
/// </summary>
/// <param name="Filter">The parsed filter model, or <see cref="FilterModel.Empty"/>.</param>
/// <param name="Search">The parsed search model, or <see cref="SearchModel.Empty"/>.</param>
/// <param name="Sort">The parsed sort model, or <see cref="SortModel.Empty"/>.</param>
/// <param name="Page">The parsed page model, or <see cref="PageModel.Empty"/>.</param>
public sealed record QueryingModel(
    FilterModel Filter,
    SearchModel Search,
    SortModel Sort,
    PageModel Page
)
{
    /// <summary>
    /// Returns a <see cref="QueryingModel"/> with all four concern models set to their
    /// respective empty sentinels — equivalent to a request with no query-string parameters.
    /// </summary>
    public static QueryingModel Empty { get; } = new(
        FilterModel.Empty,
        SearchModel.Empty,
        SortModel.Empty,
        PageModel.Empty);
}